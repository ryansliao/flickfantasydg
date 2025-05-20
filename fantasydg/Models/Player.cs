using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

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

        // Foreign key to Round
        public int RoundId { get; set; }
        public Round? Round { get; set; }
    }
}
