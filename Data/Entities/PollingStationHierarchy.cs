using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("polling_stations_hierarchy")]
    public class PollingStationHierarchy
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("voting_center_id")]
        public int VotingCenterId { get; set; }

        [Column("station_number")]
        public int StationNumber { get; set; }

        [Column("registered_voters")]
        public int RegisteredVoters { get; set; } = 0;

        [Column("votes_submitted")]
        public int VotesSubmitted { get; set; } = 0;

        [Column("turnout_rate")]
        public double TurnoutRate { get; set; } = 0;

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("last_update")]
        public DateTime? LastUpdate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(VotingCenterId))]
        public virtual VotingCenter VotingCenter { get; set; } = null!;
        public virtual ICollection<BureauAssignment> BureauAssignments { get; set; } = new List<BureauAssignment>();
    }
}