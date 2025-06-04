using System.ComponentModel.DataAnnotations.Schema;

public enum RosterStatus
{
    Starter,
    Bench,
    InjuryReserve
}

namespace fantasydg.Models
{
    public class TeamPlayer
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int PDGANumber { get; set; }
        [ForeignKey("PDGANumber")]
        public Player Player { get; set; }

        public int LeagueId { get; set; }
        public RosterStatus Status { get; set; } = RosterStatus.Starter;
    }
}
