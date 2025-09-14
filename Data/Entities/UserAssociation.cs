using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("user_associations")]
    public class UserAssociation
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("region_id")]
        public int? RegionId { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }

        [Column("commune_id")]
        public int? CommuneId { get; set; }

        [Column("polling_station_id")]
        public int? PollingStationId { get; set; }

        [Column("association_type")]
        public string AssociationType { get; set; } = "observer";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(PollingStationId))]
        public virtual PollingStation? PollingStation { get; set; }
    }
}