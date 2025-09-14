using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("administrative_divisions")]
    public class AdministrativeDivision
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (auto-référentiel)
        [ForeignKey(nameof(ParentId))]
        public virtual AdministrativeDivision? Parent { get; set; }
        public virtual ICollection<AdministrativeDivision> Children { get; set; } = new List<AdministrativeDivision>();
    }
}