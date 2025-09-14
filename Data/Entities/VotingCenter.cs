using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("voting_centers")]
    public class VotingCenter
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [Column("commune_id")]
        public int CommuneId { get; set; }

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; } = 0;

        [Column("polling_stations_count")]
        public int PollingStationsCount { get; set; } = 0;

        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CommuneId))]
        public virtual Commune Commune { get; set; } = null!;
        public virtual ICollection<PollingStationHierarchy> PollingStationsHierarchy { get; set; } = new List<PollingStationHierarchy>();
    }
}