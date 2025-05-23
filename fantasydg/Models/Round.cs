using System.Numerics;

namespace fantasydg.Models
{
    public class Round
    {
        public int RoundId { get; set; }
        public int RoundNumber { get; set; }
        public int TournamentId { get; set; }
        public string Division { get; set; } = null!;
        public Tournament Tournament { get; set; }
        public ICollection<RoundScore> RoundScores { get; set; }
    }
}
