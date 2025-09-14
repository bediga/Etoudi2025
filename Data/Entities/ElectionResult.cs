using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("election_results")]
    public class ElectionResult
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("candidate_id")]
        public int CandidateId { get; set; }

        [Column("polling_station_id")]
        public int? PollingStationId { get; set; }

        [Column("votes")]
        public int Votes { get; set; } = 0;

        [Column("percentage")]
        public double Percentage { get; set; } = 0;

        [Column("total_votes")]
        public int TotalVotes { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Column("verified")]
        public bool Verified { get; set; } = false;

        [Column("verification_notes")]
        public string? VerificationNotes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CandidateId))]
        public virtual Candidate Candidate { get; set; } = null!;

        [ForeignKey(nameof(PollingStationId))]
        public virtual PollingStation? PollingStation { get; set; }
    }
}