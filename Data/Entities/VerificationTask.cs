using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("verification_tasks")]
    public class VerificationTask
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Column("checker_id")]
        public int? CheckerId { get; set; }

        [Column("assigned_date")]
        public DateTime? AssignedDate { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("verification_notes")]
        public string? VerificationNotes { get; set; }

        [Column("completion_date")]
        public DateTime? CompletionDate { get; set; }

        [Column("verification_decision")]
        public string? VerificationDecision { get; set; }

        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CheckerId))]
        public virtual User? Checker { get; set; }

        [ForeignKey(nameof(SubmissionId))]
        public virtual ResultSubmission ResultSubmission { get; set; } = null!;

        public virtual ICollection<VerificationHistory> VerificationHistories { get; set; } = new List<VerificationHistory>();
    }
}