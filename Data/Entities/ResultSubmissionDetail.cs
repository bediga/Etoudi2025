using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("result_submission_details")]
    public class ResultSubmissionDetail
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Column("candidate_id")]
        public int CandidateId { get; set; }

        [Column("votes")]
        public int Votes { get; set; } = 0;

        [Column("percentage")]
        public double Percentage { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(SubmissionId))]
        public virtual ResultSubmission ResultSubmission { get; set; } = null!;

        [ForeignKey(nameof(CandidateId))]
        public virtual Candidate Candidate { get; set; } = null!;
    }
}