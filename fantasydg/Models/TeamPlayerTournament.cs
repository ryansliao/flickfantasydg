namespace fantasydg.Models
{
    public class TeamPlayerTournament
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int PDGANumber { get; set; }
        public Player Player { get; set; }

        public int TournamentId { get; set; }
        public string Division { get; set; }
        public Tournament Tournament { get; set; }

        public bool IsLocked { get; set; } = false;

        public string Status { get; set; }
    }
}
