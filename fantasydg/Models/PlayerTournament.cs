using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class PlayerTournament
    {
        [Key, Column(Order = 0)]
        [Required]
        public int PlayerId { get; set; }
        [Key, Column(Order = 1)]
        [Required]
        public int TournamentId { get; set; }
        [Key, Column(Order = 2)]
        [Required]
        public string Division { get; set; }

        public int Place { get; set; }
        public int TotalToPar { get; set; }
        public double Fairway { get; set; }
        public double C1InReg { get; set; }
        public double C2InReg { get; set; }
        public double Parked { get; set; }
        public double Scramble { get; set; }
        public double C1Putting { get; set; }
        public double C1xPutting { get; set; }
        public double C2Putting { get; set; }
        public double ObRate { get; set; }
        public double BirdieMinus { get; set; }
        public double DoubleBogeyPlus { get; set; }
        public double BogeyPlus { get; set; }
        public double Par { get; set; }
        public double Birdie { get; set; }
        public double EagleMinus { get; set; }
        public int PuttDistance { get; set; }
        public double StrokesGainedTotal { get; set; }
        public double StrokesGainedTeeToGreen { get; set; }
        public double StrokesGainedPutting { get; set; }
        public double StrokesGainedC1xPutting { get; set; }
        public double StrokesGainedC2Putting { get; set; }

        public Player Player { get; set; }
        public Tournament Tournament { get; set; }
    }
}
