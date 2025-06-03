using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models
{
    public class LeaguePlayerFantasyPoints
    {
        public int LeaguePlayerId { get; set; }

        public int LeagueId { get; set; }
        public League League { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int TournamentId { get; set; }
        public string Division { get; set; }
        public Tournament Tournament { get; set; }

        public float Points { get; set; }
    }
}
