using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("bureau_assignments")]
    public class BureauAssignment
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("polling_station_id")]
        public int PollingStationId { get; set; }

        [Column("assigned_by")]
        public int AssignedBy { get; set; }

        [Column("assignment_type")]
        public string AssignmentType { get; set; } = "scrutineer";

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(PollingStationId))]
        public virtual PollingStationHierarchy PollingStationHierarchy { get; set; } = null!;

        [ForeignKey(nameof(AssignedBy))]
        public virtual User AssignedByUser { get; set; } = null!;
    }
}