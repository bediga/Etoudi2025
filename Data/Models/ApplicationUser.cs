using Microsoft.AspNetCore.Identity;

namespace VcBlazor.Data.Models
{
    /// <summary>
    /// Modèle d'utilisateur personnalisé pour ASP.NET Core Identity
    /// Basé sur l'entité User existante du système électoral
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Propriétés supplémentaires spécifiques au système électoral
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? PollingStationId { get; set; }
        public string? AvatarPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? Region { get; set; }
        public string? Department { get; set; }
        public string? Arrondissement { get; set; }
        public string? Commune { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        
        // Propriété calculée pour le nom complet
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}