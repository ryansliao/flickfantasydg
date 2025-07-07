namespace fantasydg.Models.ViewModels
{
    public class LeagueTournamentPlayersViewModel
    {
        public League League { get; set; }
        public List<TeamPlayerTournament> TeamPlayerTournaments { get; set; }
        public Tournament Tournament { get; set; }
    }
}
