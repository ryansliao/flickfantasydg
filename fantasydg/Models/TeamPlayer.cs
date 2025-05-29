namespace fantasydg.Models
{
    public class TeamPlayer
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int LeagueId { get; set; } // redundant but needed to enforce uniqueness
    }
}
