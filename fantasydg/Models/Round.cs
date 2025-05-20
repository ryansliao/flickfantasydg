using System.Numerics;

namespace fantasydg.Models
{
    public class Round
    {
        public int RoundId { get; set; }
        public int RoundNumber { get; set; }
        public bool Exists { get; set; }

        // Foreign Key to Tournament
        public int TournamentId { get; set; }
        public string Division { get; set; } = null!;
        public Tournament? Tournament { get; set; }

        public List<Player> Players { get; set; } = new();
    }
}
