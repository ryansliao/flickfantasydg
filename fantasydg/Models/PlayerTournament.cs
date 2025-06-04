using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class PlayerTournament
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ResultId { get; set; }

        [Required]
        public int PDGANumber { get; set; }

        [Required]
        public int TournamentId { get; set; }

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
        public int TotalPuttDistance { get; set; }
        public int LongThrowIn {  get; set; }
        public double AvgPuttDistance { get; set; }
        public double StrokesGainedTotal { get; set; }
        public double StrokesGainedTeeToGreen { get; set; }
        public double StrokesGainedPutting { get; set; }
        public double StrokesGainedC1xPutting { get; set; }
        public double StrokesGainedC2Putting { get; set; }

        [ForeignKey("PDGANumber")]
        public Player Player { get; set; }
        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; }
    }
}
