using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("submission_results")]
    public class SubmissionResult
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Column("candidate_id")]
        public int CandidateId { get; set; }

        [Column("votes_received")]
        public int VotesReceived { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CandidateId))]
        public virtual Candidate Candidate { get; set; } = null!;

        [ForeignKey(nameof(SubmissionId))]
        public virtual ResultSubmission ResultSubmission { get; set; } = null!;
    }
}