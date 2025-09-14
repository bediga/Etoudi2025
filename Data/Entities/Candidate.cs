using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("candidates")]
    public class Candidate
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
        [Column("party")]
        public string Party { get; set; } = string.Empty;

        [Column("photo")]
        public string? Photo { get; set; }

        [Column("program")]
        public string? Program { get; set; }

        [Column("age")]
        public int? Age { get; set; }

        [Column("profession")]
        public string? Profession { get; set; }

        [Column("education")]
        public string? Education { get; set; }

        [Column("experience")]
        public string? Experience { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("website")]
        public string? Website { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("total_votes")]
        public int TotalVotes { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<ElectionResult> ElectionResults { get; set; } = new List<ElectionResult>();
        public virtual ICollection<ResultSubmissionDetail> ResultSubmissionDetails { get; set; } = new List<ResultSubmissionDetail>();
        public virtual ICollection<SubmissionResult> SubmissionResults { get; set; } = new List<SubmissionResult>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    }
}