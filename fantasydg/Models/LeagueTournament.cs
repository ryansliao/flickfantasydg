using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class LeagueTournament
    {
        [Key, Column(Order = 0)]
        public int LeagueId { get; set; }

        [Key, Column(Order = 1)]
        public int TournamentId { get; set; }

        [Key, Column(Order = 2)]
        public string Division { get; set; } = null!;

        public double Weight { get; set; } = 1;

        public League League { get; set; }
        public Tournament Tournament { get; set; }
    }
}
