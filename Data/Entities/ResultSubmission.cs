using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("result_submissions")]
    public class ResultSubmission
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("polling_station_id")]
        public int PollingStationId { get; set; }

        [Column("submitted_by")]
        public int SubmittedBy { get; set; }

        [Column("submission_type")]
        public string SubmissionType { get; set; } = "final";

        [Column("total_votes")]
        public int TotalVotes { get; set; } = 0;

        [Column("registered_voters")]
        public int RegisteredVoters { get; set; } = 0;

        [Column("turnout_rate")]
        public double TurnoutRate { get; set; } = 0;

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        [Column("verified_by")]
        public int? VerifiedBy { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(SubmittedBy))]
        public virtual User SubmittedByUser { get; set; } = null!;

        [ForeignKey(nameof(VerifiedBy))]
        public virtual User? VerifiedByUser { get; set; }

        [ForeignKey(nameof(PollingStationId))]
        public virtual PollingStation PollingStation { get; set; } = null!;

        public virtual ICollection<ResultSubmissionDetail> ResultSubmissionDetails { get; set; } = new List<ResultSubmissionDetail>();
        public virtual ICollection<SubmissionResult> SubmissionResults { get; set; } = new List<SubmissionResult>();
        public virtual ICollection<SubmissionDocument> SubmissionDocuments { get; set; } = new List<SubmissionDocument>();
        public virtual ICollection<VerificationTask> VerificationTasks { get; set; } = new List<VerificationTask>();
    }
}