using Microsoft.AspNetCore.Mvc;
using VcBlazor.Services;

namespace VcBlazor.Controllers
{
    public class ImportController : Controller
    {
        private readonly CsvImportService _importService;
        private readonly ILogger<ImportController> _logger;

        public ImportController(CsvImportService importService, ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        // GET: Import
        public IActionResult Index()
        {
            return View();
        }

        // POST: Import/UploadCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["Error"] = "Veuillez sélectionner un fichier CSV.";
                return RedirectToAction(nameof(Index));
            }

            // Vérifier l'extension du fichier
            var extension = Path.GetExtension(csvFile.FileName).ToLower();
            if (extension != ".csv")
            {
                TempData["Error"] = "Seuls les fichiers CSV sont autorisés.";
                return RedirectToAction(nameof(Index));
            }

            // Vérifier la taille du fichier (max 50MB)
            if (csvFile.Length > 50 * 1024 * 1024)
            {
                TempData["Error"] = "Le fichier est trop volumineux. Taille maximale: 50MB.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = csvFile.OpenReadStream();
                var result = await _importService.ImportFromCsvAsync(stream);

                if (result.Success)
                {
                    TempData["Success"] = $"Import réussi ! " +
                        $"Régions: {result.RegionsImported}, " +
                        $"Départements: {result.DepartmentsImported}, " +
                        $"Arrondissements: {result.ArrondissementsImported}, " +
                        $"Communes: {result.CommunesImported}. " +
                        $"Total lignes traitées: {result.TotalRowsProcessed}";
                    
                    if (result.ErrorsCount > 0)
                    {
                        TempData["Warning"] = $"Attention: {result.ErrorsCount} erreurs détectées lors du traitement.";
                    }
                }
                else
                {
                    TempData["Error"] = $"Erreur lors de l'import: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload du fichier CSV");
                TempData["Error"] = "Une erreur inattendue s'est produite lors de l'import.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Import/Sample
        public IActionResult DownloadSample()
        {
            var csvContent = "Région,Département,Arrondissement,Commune\n" +
                           "Centre,Mfoundi,Yaoundé I,Yaoundé I\n" +
                           "Centre,Mfoundi,Yaoundé II,Yaoundé II\n" +
                           "Littoral,Wouri,Douala I,Douala I\n" +
                           "Littoral,Wouri,Douala II,Douala II\n" +
                           "Ouest,Mifi,Bafoussam I,Bafoussam I";

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            return File(bytes, "text/csv", "exemple_bureaux_vote.csv");
        }
    }
}