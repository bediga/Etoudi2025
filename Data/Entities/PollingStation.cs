using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("polling_stations")]
    public class PollingStation
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("region")]
        public string? Region { get; set; }

        [Column("department")]
        public string? Department { get; set; }

        [Column("commune")]
        public string? Commune { get; set; }

        [Column("arrondissement")]
        public string? Arrondissement { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("registered_voters")]
        public int RegisteredVoters { get; set; } = 0;

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("votes_submitted")]
        public int VotesSubmitted { get; set; } = 0;

        [Column("turnout_rate")]
        public double TurnoutRate { get; set; } = 0;

        [Column("last_update")]
        public DateTime? LastUpdate { get; set; }

        [Column("scrutineers_count")]
        public int ScrutineersCount { get; set; } = 0;

        [Column("observers_count")]
        public int ObserversCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<ElectionResult> ElectionResults { get; set; } = new List<ElectionResult>();
        public virtual ICollection<ResultSubmission> ResultSubmissions { get; set; } = new List<ResultSubmission>();
        public virtual ICollection<HourlyTurnout> HourlyTurnouts { get; set; } = new List<HourlyTurnout>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<UserAssociation> UserAssociations { get; set; } = new List<UserAssociation>();
    }
}