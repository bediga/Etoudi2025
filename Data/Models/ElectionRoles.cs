namespace VcBlazor.Data.Models
{
    /// <summary>
    /// Définition des rôles utilisateur pour le système électoral
    /// </summary>
    public static class ElectionRoles
    {
        /// <summary>
        /// Super Administrateur - Accès total système, gestion des autres admins
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";
        
        /// <summary>
        /// Administrateur du système - Accès complet à toutes les fonctionnalités
        /// </summary>
        public const string Administrator = "Administrator";
        
        /// <summary>
        /// Superviseur électoral - Supervision et validation des résultats
        /// </summary>
        public const string Supervisor = "Supervisor";
        
        /// <summary>
        /// Opérateur de saisie - Saisie et gestion des données électorales
        /// </summary>
        public const string Operator = "Operator";
        
        /// <summary>
        /// Observateur - Accès en lecture seule aux données
        /// </summary>
        public const string Observer = "Observer";
        
        /// <summary>
        /// Président de bureau de vote - Gestion spécifique d'un bureau
        /// </summary>
        public const string PresidentBureau = "PresidentBureau";
        
        /// <summary>
        /// Assesseur - Assistance dans un bureau de vote
        /// </summary>
        public const string Assesseur = "Assesseur";

        /// <summary>
        /// Liste de tous les rôles disponibles
        /// </summary>
        public static readonly string[] AllRoles = 
        {
            SuperAdmin,
            Administrator,
            Supervisor,
            Operator,
            Observer,
            PresidentBureau,
            Assesseur
        };

        /// <summary>
        /// Rôles avec privilèges élevés (administration et supervision)
        /// </summary>
        public static readonly string[] HighPrivilegeRoles = 
        {
            SuperAdmin,
            Administrator,
            Supervisor
        };

        /// <summary>
        /// Rôles opérationnels (saisie et gestion courante)
        /// </summary>
        public static readonly string[] OperationalRoles = 
        {
            Operator,
            PresidentBureau,
            Assesseur
        };
        
        /// <summary>
        /// Rôles avec accès en lecture seule
        /// </summary>
        public static readonly string[] ReadOnlyRoles = 
        {
            Observer
        };
    }
}