using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("polling_station_id")]
        public int? PollingStationId { get; set; }

        [Column("avatarPath")]
        public string? AvatarPath { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("region")]
        public string? Region { get; set; }

        [Column("department")]
        public string? Department { get; set; }

        [Column("arrondissement")]
        public string? Arrondissement { get; set; }

        [Column("commune")]
        public string? Commune { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("must_change_password")]
        public bool MustChangePassword { get; set; } = false;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("password_changed_at")]
        public DateTime? PasswordChangedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PollingStationId))]
        public virtual PollingStation? PollingStation { get; set; }
        public virtual ICollection<ResultSubmission> SubmittedResults { get; set; } = new List<ResultSubmission>();
        public virtual ICollection<ResultSubmission> VerifiedResults { get; set; } = new List<ResultSubmission>();
        public virtual ICollection<VerificationTask> CheckerTasks { get; set; } = new List<VerificationTask>();
        public virtual ICollection<VerificationHistory> VerificationHistories { get; set; } = new List<VerificationHistory>();
        public virtual ICollection<HourlyTurnout> RecordedTurnouts { get; set; } = new List<HourlyTurnout>();
        public virtual ICollection<BureauAssignment> UserAssignments { get; set; } = new List<BureauAssignment>();
        public virtual ICollection<BureauAssignment> AssignedByUser { get; set; } = new List<BureauAssignment>();
        public virtual ICollection<Result> SubmittedResultsSimple { get; set; } = new List<Result>();
        public virtual ICollection<UserAssociation> UserAssociations { get; set; } = new List<UserAssociation>();
    }
}