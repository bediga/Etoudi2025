using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VcBlazor.Data.Entities
{
    [Table("hourly_turnout")]
    public class HourlyTurnout
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("polling_station_id")]
        public int PollingStationId { get; set; }

        [Column("hour")]
        public int Hour { get; set; }

        [Column("voters_count")]
        public int VotersCount { get; set; } = 0;

        [Column("cumulative_count")]
        public int CumulativeCount { get; set; } = 0;

        [Column("turnout_rate")]
        public double TurnoutRate { get; set; } = 0;

        [Column("recorded_by")]
        public int? RecordedBy { get; set; }

        [Column("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(RecordedBy))]
        public virtual User? RecordedByUser { get; set; }

        [ForeignKey(nameof(PollingStationId))]
        public virtual PollingStation PollingStation { get; set; } = null!;
    }
}