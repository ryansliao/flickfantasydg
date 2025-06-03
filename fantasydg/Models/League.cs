using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class League
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeagueId { get; set; }

        [Required]
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public int PlayerNumber {  get; set; }

        // Scoring settings
        public double PlacementWeight { get; set; } = -0.05;
        public double FairwayWeight { get; set; } = 0;
        public double C1InRegWeight { get; set; } = 0.075;
        public double C2InRegWeight { get; set; } = 0.025;
        public double ParkedWeight { get; set; } = 0.15;
        public double ScrambleWeight { get; set; } = 0;
        public double C1PuttWeight { get; set; } = 0;
        public double C1xPuttWeight { get; set; } = 0.02;
        public double C2PuttWeight { get; set; } = 0.03;
        public double OBWeight { get; set; } = -0.15;
        public double BirdieWeight { get; set; } = 0;
        public double BirdieMinusWeight { get; set; } = 0.1;
        public double EagleMinusWeight { get; set; } = 0.5;
        public double ParWeight { get; set; } = 0;
        public double BogeyPlusWeight { get; set; } = -0.1;
        public double DoubleBogeyPlusWeight { get; set; } = -0.25;
        public double TotalPuttDistWeight { get; set; } = 0.01;
        public double AvgPuttDistWeight { get; set; } = 0;
        public double LongThrowInWeight { get; set; } = 0;
        public double TotalSGWeight { get; set; } = 0;
        public double PuttingSGWeight { get; set; } = 0;
        public double TeeToGreenSGWeight { get; set; } = 0;
        public double C1xSGWeight { get; set; } = 0;
        public double C2SGWeight { get; set; } = 0;

        [ValidateNever]
        public ApplicationUser Owner { get; set; }
        [ValidateNever]
        public ICollection<LeagueMember> Members { get; set; }
        [ValidateNever]
        public ICollection<Team> Teams { get; set; }
    }
}
