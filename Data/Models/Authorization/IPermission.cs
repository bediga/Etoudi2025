namespace VcBlazor.Data.Models.Authorization
{
    /// <summary>
    /// Interface pour définir une permission dans le système
    /// </summary>
    public interface IPermission
    {
        /// <summary>
        /// Identifiant unique de la permission
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Nom d'affichage de la permission
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Description de ce que permet cette permission
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Catégorie/Module auquel appartient cette permission
        /// </summary>
        string Category { get; }
        
        /// <summary>
        /// Niveau de risque de cette permission (Low, Medium, High, Critical)
        /// </summary>
        PermissionLevel Level { get; }
    }

    /// <summary>
    /// Niveaux de permission
    /// </summary>
    public enum PermissionLevel
    {
        Low = 1,        // Lecture simple
        Medium = 2,     // Écriture/Modification
        High = 3,       // Suppression/Gestion
        Critical = 4    // Administration système
    }

    /// <summary>
    /// Types d'actions possibles
    /// </summary>
    public enum ActionType
    {
        Read,       // Lecture/Consultation
        Create,     // Création
        Update,     // Modification
        Delete,     // Suppression
        Manage,     // Gestion complète
        Export,     // Export de données
        Import,     // Import de données
        Verify,     // Vérification/Validation
        Publish,    // Publication
        Admin       // Administration
    }

    /// <summary>
    /// Implémentation concrète d'une permission
    /// </summary>
    public class Permission : IPermission
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public PermissionLevel Level { get; set; }

        public Permission() { }

        public Permission(string id, string displayName, string description, string category, PermissionLevel level)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Category = category;
            Level = level;
        }

        /// <summary>
        /// Crée une permission selon un pattern standardisé
        /// </summary>
        public static Permission Create(string resource, ActionType action, PermissionLevel level = PermissionLevel.Medium)
        {
            var id = $"{resource.ToLower()}.{action.ToString().ToLower()}";
            var displayName = $"{GetActionDisplayName(action)} {resource}";
            var description = $"Permet de {GetActionDescription(action)} les {resource.ToLower()}";
            
            return new Permission(id, displayName, description, resource, level);
        }

        private static string GetActionDisplayName(ActionType action)
        {
            return action switch
            {
                ActionType.Read => "Consulter",
                ActionType.Create => "Créer",
                ActionType.Update => "Modifier",
                ActionType.Delete => "Supprimer",
                ActionType.Manage => "Gérer",
                ActionType.Export => "Exporter",
                ActionType.Import => "Importer",
                ActionType.Verify => "Vérifier",
                ActionType.Publish => "Publier",
                ActionType.Admin => "Administrer",
                _ => action.ToString()
            };
        }

        private static string GetActionDescription(ActionType action)
        {
            return action switch
            {
                ActionType.Read => "consulter et voir",
                ActionType.Create => "créer de nouveaux",
                ActionType.Update => "modifier",
                ActionType.Delete => "supprimer",
                ActionType.Manage => "gérer complètement",
                ActionType.Export => "exporter des données de",
                ActionType.Import => "importer des données vers",
                ActionType.Verify => "vérifier et valider",
                ActionType.Publish => "publier",
                ActionType.Admin => "administrer",
                _ => action.ToString().ToLower()
            };
        }
    }
}