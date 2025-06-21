namespace fantasydg.Models.ViewModels
{
    public class LeagueLeaderboardViewModel
    {
        public League League { get; set; }
        public List<Tournament> Tournaments { get; set; }
        public List<TeamRow> Teams { get; set; }

        public class TeamRow
        {
            public string TeamName { get; set; }
            public string OwnerName { get; set; }
            public Dictionary<int, double> PointsByTournament { get; set; } = new();
            public double TotalPoints { get; set; }
        }
    }
}
