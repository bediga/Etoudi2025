using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("verification_history")]
    public class VerificationHistory
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("checker_id")]
        public int CheckerId { get; set; }

        [Required]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CheckerId))]
        public virtual User Checker { get; set; } = null!;

        [ForeignKey(nameof(TaskId))]
        public virtual VerificationTask VerificationTask { get; set; } = null!;
    }
}