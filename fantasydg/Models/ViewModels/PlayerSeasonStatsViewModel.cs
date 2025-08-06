using System.Security.Permissions;

namespace fantasydg.Models.ViewModels
{
    public class PlayerSeasonStatsViewModel
    {
        public Player Player { get; set; }
        public int PDGANumber { get; set; }
        public double FantasyPoints { get; set; }
        public int WorldRanking { get; set; }
        public double Place { get; set; }
        public double Fairway { get; set; }
        public double C1InReg { get; set; }
        public double C2InReg { get; set; }
        public double Parked { get; set; }
        public double Scramble { get; set; }
        public double C1Putting { get; set; }
        public double C1xPutting { get; set; }
        public double C2Putting { get; set; }
        public double ObRate { get; set; }
        public double Par { get; set; }
        public double Birdie { get; set; }
        public double BirdieMinus { get; set; }
        public double EagleMinus { get; set; }
        public double BogeyPlus { get; set; }
        public double DoubleBogeyPlus { get; set; }
        public double TotalPuttDistance { get; set; }
        public double AvgPuttDistance { get; set; }
        public int LongThrowIn { get; set; }
        public double StrokesGainedTotal { get; set; }
        public double StrokesGainedPutting { get; set; }
        public double StrokesGainedTeeToGreen { get; set; }
        public double StrokesGainedC1xPutting { get; set; }
        public double StrokesGainedC2Putting { get; set; }
    }

}
