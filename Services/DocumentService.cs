using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using VcBlazor.Data;
using VcBlazor.Data.Entities;

namespace VcBlazor.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly Vc2025DbContext _context;
        private readonly ILogger<DocumentService> _logger;

        // Limites et types de fichiers autorisés
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly string[] AllowedDocumentTypes = { ".pdf", ".doc", ".docx", ".txt", ".rtf" };
        private readonly Dictionary<string, string> MimeTypeIcons = new()
        {
            { "application/pdf", "fas fa-file-pdf text-danger" },
            { "image/jpeg", "fas fa-file-image text-success" },
            { "image/png", "fas fa-file-image text-success" },
            { "image/gif", "fas fa-file-image text-success" },
            { "application/msword", "fas fa-file-word text-primary" },
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "fas fa-file-word text-primary" },
            { "text/plain", "fas fa-file-alt text-secondary" },
            { "application/rtf", "fas fa-file-alt text-secondary" }
        };

        public DocumentService(IWebHostEnvironment environment, Vc2025DbContext context, ILogger<DocumentService> logger)
        {
            _environment = environment;
            _context = context;
            _logger = logger;
            
            // Créer les dossiers de stockage si nécessaire
            EnsureStorageDirectoriesExist();
        }

        /// <summary>
        /// Créer un enregistrement de document et sauvegarder le fichier
        /// </summary>
        public async Task<SubmissionDocument> CreateDocumentRecordAsync(
            int submissionId, 
            IFormFile file, 
            string documentType, 
            string? description = null, 
            int uploadedBy = 1)
        {
            // Validation du fichier
            if (!ValidateFile(file, out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Générer un nom de fichier sécurisé
            var secureFileName = GenerateSecureFileName(file.FileName);
            var storagePath = GetStoragePath(documentType);
            var fullPath = Path.Combine(_environment.WebRootPath, storagePath, secureFileName);

            // Créer le dossier si nécessaire
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            // Sauvegarder le fichier
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Calculer le checksum
            var checksum = await CalculateChecksumAsync(file);

            // Créer l'enregistrement en base
            var document = new SubmissionDocument
            {
                SubmissionId = submissionId,
                FileName = secureFileName,
                OriginalFileName = file.FileName,
                FilePath = Path.Combine(storagePath, secureFileName),
                FileSize = file.Length,
                MimeType = file.ContentType,
                DocumentType = documentType,
                Description = description,
                IsImage = IsImageFile(file.ContentType ?? ""),
                Checksum = checksum,
                UploadedBy = uploadedBy,
                UploadDate = DateTime.UtcNow,
                Status = "active"
            };

            _context.SubmissionDocuments.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Document {file.FileName} sauvegardé avec succès (ID: {document.Id})");
            
            return document;
        }

        /// <summary>
        /// Récupère tous les documents d'une soumission
        /// </summary>
        public async Task<List<SubmissionDocument>> GetSubmissionDocumentsAsync(int submissionId)
        {
            return await _context.SubmissionDocuments
                .Include(d => d.UploadedByUser)
                .Where(d => d.SubmissionId == submissionId && d.Status == "active")
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère un document par son ID
        /// </summary>
        public async Task<SubmissionDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.SubmissionDocuments
                .Include(d => d.UploadedByUser)
                .Include(d => d.Submission)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Status == "active");
        }

        /// <summary>
        /// Récupère le contenu d'un fichier pour téléchargement
        /// </summary>
        public async Task<(byte[] Content, string MimeType, string FileName)> GetDocumentContentAsync(int documentId)
        {
            var document = await GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                throw new FileNotFoundException("Document introuvable");
            }

            var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Fichier physique introuvable: {fullPath}");
            }

            var content = await File.ReadAllBytesAsync(fullPath);
            return (content, document.MimeType ?? "application/octet-stream", document.OriginalFileName);
        }

        /// <summary>
        /// Supprime un document (fichier + base de données)
        /// </summary>
        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                return false;
            }

            try
            {
                // Supprimer le fichier physique
                var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                // Marquer comme supprimé en base (soft delete)
                document.Status = "deleted";
                document.DeletedAt = DateTime.UtcNow;
                
                _context.Update(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Document {document.OriginalFileName} supprimé avec succès (ID: {documentId})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la suppression du document {documentId}");
                return false;
            }
        }

        /// <summary>
        /// Valide qu'un fichier est acceptable
        /// </summary>
        public bool ValidateFile(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Aucun fichier sélectionné";
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                errorMessage = $"La taille du fichier ({file.Length / (1024 * 1024)} MB) dépasse la limite autorisée ({MaxFileSize / (1024 * 1024)} MB)";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allAllowedTypes = AllowedImageTypes.Concat(AllowedDocumentTypes).ToArray();
            
            if (!allAllowedTypes.Contains(extension))
            {
                errorMessage = $"Type de fichier non autorisé: {extension}. Types autorisés: {string.Join(", ", allAllowedTypes)}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtient l'icône CSS appropriée pour un type MIME
        /// </summary>
        public string GetFileIcon(string mimeType)
        {
            return MimeTypeIcons.TryGetValue(mimeType, out var icon) 
                ? icon 
                : "fas fa-file text-muted";
        }

        /// <summary>
        /// Calcule le checksum MD5 d'un fichier
        /// </summary>
        public async Task<string> CalculateChecksumAsync(IFormFile file)
        {
            using var md5 = MD5.Create();
            using var stream = file.OpenReadStream();
            var hash = await Task.Run(() => md5.ComputeHash(stream));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Génère un nom de fichier sécurisé et unique
        /// </summary>
        public string GenerateSecureFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            
            // Nettoyer le nom de fichier
            var cleanName = string.Join("_", nameWithoutExt.Split(Path.GetInvalidFileNameChars()));
            
            // Ajouter un timestamp pour l'unicité
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            
            return $"{cleanName}_{timestamp}_{uniqueId}{extension}";
        }

        /// <summary>
        /// Vérifie si un fichier est une image
        /// </summary>
        public bool IsImageFile(string mimeType)
        {
            return mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtient le chemin de stockage pour un type de document
        /// </summary>
        public string GetStoragePath(string documentType)
        {
            return documentType.ToLowerInvariant() switch
            {
                "pv" => "uploads/documents/pv",
                "photo" => "uploads/documents/photos",
                "compte-rendu" => "uploads/documents/reports",
                "rapport" => "uploads/documents/reports",
                _ => "uploads/documents/other"
            };
        }

        /// <summary>
        /// Crée les dossiers de stockage s'ils n'existent pas
        /// </summary>
        private void EnsureStorageDirectoriesExist()
        {
            var basePath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
            var subdirectories = new[] { "pv", "photos", "reports", "other" };

            Directory.CreateDirectory(basePath);
            
            foreach (var subdir in subdirectories)
            {
                Directory.CreateDirectory(Path.Combine(basePath, subdir));
            }
        }
    }
}