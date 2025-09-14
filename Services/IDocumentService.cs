using VcBlazor.Data.Entities;

namespace VcBlazor.Services
{
    /// <summary>
    /// Service pour la gestion des documents de soumission (PV, photos, rapports)
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Sauvegarde un fichier et crée l'enregistrement en base de données
        /// </summary>
        Task<SubmissionDocument> CreateDocumentRecordAsync(
            int submissionId, 
            IFormFile file, 
            string documentType, 
            string? description = null, 
            int uploadedBy = 1);

        /// <summary>
        /// Récupère tous les documents d'une soumission
        /// </summary>
        Task<List<SubmissionDocument>> GetSubmissionDocumentsAsync(int submissionId);

        /// <summary>
        /// Récupère un document par son ID avec ses informations
        /// </summary>
        Task<SubmissionDocument?> GetDocumentByIdAsync(int documentId);

        /// <summary>
        /// Récupère le contenu d'un fichier pour téléchargement
        /// </summary>
        Task<(byte[] Content, string MimeType, string FileName)> GetDocumentContentAsync(int documentId);

        /// <summary>
        /// Supprime un document (fichier + base de données)
        /// </summary>
        Task<bool> DeleteDocumentAsync(int documentId);

        /// <summary>
        /// Valide qu'un fichier est acceptable (type, taille, etc.)
        /// </summary>
        bool ValidateFile(IFormFile file, out string errorMessage);

        /// <summary>
        /// Obtient l'icône CSS appropriée pour un type MIME
        /// </summary>
        string GetFileIcon(string mimeType);

        /// <summary>
        /// Calcule le checksum MD5 d'un fichier
        /// </summary>
        Task<string> CalculateChecksumAsync(IFormFile file);

        /// <summary>
        /// Génère un nom de fichier sécurisé et unique
        /// </summary>
        string GenerateSecureFileName(string originalFileName);

        /// <summary>
        /// Vérifie si un fichier est une image
        /// </summary>
        bool IsImageFile(string mimeType);

        /// <summary>
        /// Obtient le chemin de stockage pour un type de document
        /// </summary>
        string GetStoragePath(string documentType);
    }
}