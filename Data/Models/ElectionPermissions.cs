using VcBlazor.Data.Models.Authorization;

namespace VcBlazor.Data.Models
{
    /// <summary>
    /// Classe de compatibilité pour l'ancien système de permissions
    /// Utilise maintenant le PermissionRegistry générique
    /// </summary>
    [Obsolete("Utilisez PermissionRegistry et IAuthorizationService à la place")]
    public static class ElectionPermissions
    {
        /// <summary>
        /// Vérifie si un rôle a une permission spécifique
        /// </summary>
        /// <param name="role">Le rôle à vérifier</param>
        /// <param name="permission">La permission à vérifier</param>
        /// <returns>True si le rôle a la permission</returns>
        public static bool HasPermission(string role, string permission)
        {
            return PermissionRegistry.HasPermission(role, permission);
        }

        /// <summary>
        /// Obtient toutes les permissions pour un rôle donné
        /// </summary>
        /// <param name="role">Le rôle</param>
        /// <returns>Liste des permissions</returns>
        public static string[] GetPermissions(string role)
        {
            return PermissionRegistry.GetRolePermissions(role);
        }
        
        // Constantes de compatibilité - utilisez PermissionRegistry à la place
        public const string ManageUsers = "users.manage";
        public const string ViewUsers = "users.read";
        public const string CreateUsers = "users.create";
        public const string EditUsers = "users.update";
        public const string DeleteUsers = "users.delete";

        public const string ManageCandidates = "candidates.manage";
        public const string ViewCandidates = "candidates.read";
        public const string CreateCandidates = "candidates.create";
        public const string EditCandidates = "candidates.update";
        public const string DeleteCandidates = "candidates.delete";

        public const string ManageResults = "results.manage";
        public const string ViewResults = "results.read";
        public const string CreateResults = "results.create";
        public const string EditResults = "results.update";
        public const string DeleteResults = "results.delete";
        public const string VerifyResults = "results.verify";
        public const string PublishResults = "results.publish";

        public const string ManagePollingStations = "pollingstations.manage";
        public const string ViewPollingStations = "pollingstations.read";
        public const string CreatePollingStations = "pollingstations.create";
        public const string EditPollingStations = "pollingstations.update";
        public const string DeletePollingStations = "pollingstations.delete";

        public const string ViewReports = "reports.read";
        public const string ExportData = "reports.export";
        
        public const string AccessAdmin = "system.admin";
        public const string ManageSettings = "system.manage";
        public const string ViewSystemStats = "system.read";
    }
}