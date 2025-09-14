using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Services
{
    public class CsvImportService
    {
        private readonly Vc2025DbContext _context;
        private readonly ILogger<CsvImportService> _logger;

        public CsvImportService(Vc2025DbContext context, ILogger<CsvImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ImportResult> ImportFromCsvAsync(Stream csvStream)
        {
            var result = new ImportResult();
            
            try
            {
                using var reader = new StreamReader(csvStream, Encoding.UTF8);
                var csvContent = await reader.ReadToEndAsync();
                var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                {
                    result.ErrorMessage = "Le fichier CSV doit contenir au moins une ligne d'en-tête et une ligne de données.";
                    return result;
                }

                // Analyser l'en-tête pour détecter les colonnes
                var headers = ParseCsvLine(lines[0]);
                var columnMapping = DetectColumnMapping(headers);

                if (!ValidateColumnMapping(columnMapping))
                {
                    var detectedColumns = string.Join(", ", headers);
                    result.ErrorMessage = $"Colonnes requises manquantes. Colonnes détectées: [{detectedColumns}]. " +
                                        $"Le fichier doit contenir au minimum: une colonne 'Région' et une colonne 'Département'. " +
                                        $"Formats acceptés: Région/Region, Département/Departement/Dept, Arrondissement/Arr, Commune/Comm.";
                    return result;
                }

                // Log des colonnes détectées
                _logger.LogInformation($"Colonnes détectées - Région: {(columnMapping.RegionIndex >= 0 ? headers[columnMapping.RegionIndex] : "Non trouvée")}, " +
                                     $"Département: {(columnMapping.DepartmentIndex >= 0 ? headers[columnMapping.DepartmentIndex] : "Non trouvée")}, " +
                                     $"Arrondissement: {(columnMapping.ArrondissementIndex >= 0 ? headers[columnMapping.ArrondissementIndex] : "Non trouvée")}, " +
                                     $"Commune: {(columnMapping.CommuneIndex >= 0 ? headers[columnMapping.CommuneIndex] : "Non trouvée")}");

                // Traiter les données
                var regions = new Dictionary<string, Region>();
                var departments = new Dictionary<string, Department>();
                var arrondissements = new Dictionary<string, Arrondissement>();

                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var values = ParseCsvLine(lines[i]);
                        if (values.Length < headers.Length) continue;

                        var rowData = new CsvRowData
                        {
                            Region = GetValue(values, columnMapping.RegionIndex),
                            Department = GetValue(values, columnMapping.DepartmentIndex),
                            Arrondissement = GetValue(values, columnMapping.ArrondissementIndex),
                            Commune = GetValue(values, columnMapping.CommuneIndex)
                        };

                        if (string.IsNullOrWhiteSpace(rowData.Region) || 
                            string.IsNullOrWhiteSpace(rowData.Department)) continue;

                        // Traiter la région
                        var region = await ProcessRegionAsync(rowData.Region, regions);
                        
                        // Traiter le département
                        var department = await ProcessDepartmentAsync(rowData.Department, region, departments);
                        
                        // Traiter l'arrondissement si présent
                        if (!string.IsNullOrWhiteSpace(rowData.Arrondissement))
                        {
                            var arrondissement = await ProcessArrondissementAsync(rowData.Arrondissement, department, arrondissements);
                            
                            // Traiter la commune si présente
                            if (!string.IsNullOrWhiteSpace(rowData.Commune))
                            {
                                await ProcessCommuneAsync(rowData.Commune, arrondissement);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Erreur lors du traitement de la ligne {i + 1}: {ex.Message}");
                        result.ErrorsCount++;
                    }
                }

                await _context.SaveChangesAsync();
                
                // Compter les entités créées/mises à jour
                result.RegionsImported = regions.Count;
                result.DepartmentsImported = departments.Count;
                result.ArrondissementsImported = arrondissements.Count;
                // Pour les communes, comptons celles ajoutées dans cette session
                var communesCount = 0;
                foreach (var arr in arrondissements.Values)
                {
                    communesCount += await _context.Communes.CountAsync(c => c.ArrondissementId == arr.Id);
                }
                result.CommunesImported = communesCount;
                
                result.Success = true;
                result.TotalRowsProcessed = lines.Length - 1;

                _logger.LogInformation($"Import terminé: {result.RegionsImported} régions, {result.DepartmentsImported} départements, " +
                                     $"{result.ArrondissementsImported} arrondissements, {result.CommunesImported} communes importés.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'import CSV");
                result.ErrorMessage = $"Erreur lors de l'import: {ex.Message}";
            }

            return result;
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        private ColumnMapping DetectColumnMapping(string[] headers)
        {
            var mapping = new ColumnMapping();

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i].ToLower().Trim().Replace(" ", "");

                // Détection flexible des colonnes région
                if (header.Contains("région") || header.Contains("region") || 
                    header.Contains("reg") || header == "r")
                    mapping.RegionIndex = i;
                
                // Détection flexible des colonnes département
                else if (header.Contains("département") || header.Contains("departement") || 
                         header.Contains("dept") || header.Contains("dep") || header == "d")
                    mapping.DepartmentIndex = i;
                
                // Détection flexible des colonnes arrondissement
                else if (header.Contains("arrondissement") || header.Contains("arrt") || 
                         header.Contains("arrdt") || header.Contains("arr") || header == "a")
                    mapping.ArrondissementIndex = i;
                
                // Détection flexible des colonnes commune
                else if (header.Contains("commune") || header.Contains("comm") || 
                         header.Contains("city") || header == "c")
                    mapping.CommuneIndex = i;
            }

            return mapping;
        }

        private bool ValidateColumnMapping(ColumnMapping mapping)
        {
            // Nous avons besoin au minimum d'une région et d'un département
            // Les arrondissements et communes sont optionnels
            return mapping.RegionIndex >= 0 && mapping.DepartmentIndex >= 0;
        }

        private string GetValue(string[] values, int index)
        {
            return index >= 0 && index < values.Length ? values[index].Trim('"', ' ') : string.Empty;
        }

        private async Task<Region> ProcessRegionAsync(string regionName, Dictionary<string, Region> cache)
        {
            if (cache.TryGetValue(regionName, out var cachedRegion))
                return cachedRegion;

            var existing = await _context.Regions.FirstOrDefaultAsync(r => r.Name == regionName);
            if (existing != null)
            {
                cache[regionName] = existing;
                return existing;
            }

            var region = new Region
            {
                Name = regionName,
                Code = GenerateCode(regionName),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Regions.Add(region);
            cache[regionName] = region;
            return region;
        }

        private async Task<Department> ProcessDepartmentAsync(string departmentName, Region region, Dictionary<string, Department> cache)
        {
            var key = $"{region.Name}_{departmentName}";
            if (cache.TryGetValue(key, out var cachedDepartment))
                return cachedDepartment;

            var existing = await _context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName && d.Region == region);
            if (existing != null)
            {
                cache[key] = existing;
                return existing;
            }

            var department = new Department
            {
                Name = departmentName,
                Code = GenerateCode(departmentName),
                Region = region,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            cache[key] = department;
            return department;
        }

        private async Task<Arrondissement> ProcessArrondissementAsync(string arrondissementName, Department department, Dictionary<string, Arrondissement> cache)
        {
            var key = $"{department.Name}_{arrondissementName}";
            if (cache.TryGetValue(key, out var cachedArrondissement))
                return cachedArrondissement;

            var existing = await _context.Arrondissements.FirstOrDefaultAsync(a => a.Name == arrondissementName && a.Department == department);
            if (existing != null)
            {
                cache[key] = existing;
                return existing;
            }

            var arrondissement = new Arrondissement
            {
                Name = arrondissementName,
                Code = GenerateCode(arrondissementName),
                Department = department,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Arrondissements.Add(arrondissement);
            cache[key] = arrondissement;
            return arrondissement;
        }

        private async Task<Commune> ProcessCommuneAsync(string communeName, Arrondissement arrondissement)
        {
            var existing = await _context.Communes.FirstOrDefaultAsync(c => c.Name == communeName && c.Arrondissement == arrondissement);
            if (existing != null)
                return existing;

            var commune = new Commune
            {
                Name = communeName,
                Code = GenerateCode(communeName),
                Arrondissement = arrondissement,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Communes.Add(commune);
            return commune;
        }

        private string GenerateCode(string name)
        {
            // Génère un code basé sur les 3 premières lettres du nom
            return name.Length >= 3 ? name.Substring(0, 3).ToUpper() : name.ToUpper();
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalRowsProcessed { get; set; }
        public int RegionsImported { get; set; }
        public int DepartmentsImported { get; set; }
        public int ArrondissementsImported { get; set; }
        public int CommunesImported { get; set; }
        public int ErrorsCount { get; set; }
    }

    public class ColumnMapping
    {
        public int RegionIndex { get; set; } = -1;
        public int DepartmentIndex { get; set; } = -1;
        public int ArrondissementIndex { get; set; } = -1;
        public int CommuneIndex { get; set; } = -1;
    }

    public class CsvRowData
    {
        public string Region { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Arrondissement { get; set; } = string.Empty;
        public string Commune { get; set; } = string.Empty;
    }
}