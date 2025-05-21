using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class Player
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } // PDGA ID, reused across rounds

        [Key, Column(Order = 1)]
        public int RoundId { get; set; }

        public string? Name { get; set; }
        public int Place { get; set; }
        public int TournamentScore { get; set; }
        public int RoundScore { get; set; }
        public double Fairway { get; set; }
        public double C1InReg { get; set; }
        public double C2InReg { get; set; }
        public double Parked { get; set; }
        public double Scramble { get; set; }
        public double C1Putting { get; set; }
        public double C1xPutting { get; set; }
        public double C2Putting { get; set; }
        public double ObRate { get; set; }
        public double BirdiePlus { get; set; }
        public double DoubleBogeyPlus { get; set; }
        public double BogeyPlus { get; set; }
        public double Par { get; set; }
        public double Birdie { get; set; }
        public double EaglePlus { get; set; }
        public int PuttDistance { get; set; }
        public double StrokesGainedTotal { get; set; }
        public double StrokesGainedTeeToGreen { get; set; }
        public double StrokesGainedPutting { get; set; }
        public double StrokesGainedC1xPutting { get; set; }
        public double StrokesGainedC2Putting { get; set; }

        // Foreign key to Round
        public Round? Round { get; set; }
    }
}
