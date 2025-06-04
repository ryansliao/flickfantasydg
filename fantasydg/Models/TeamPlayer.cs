using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class TeamPlayer
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int PDGANumber { get; set; }
        [ForeignKey("PDGANumber")]
        public Player Player { get; set; }

        public int LeagueId { get; set; } // redundant but needed to enforce uniqueness
    }
}
