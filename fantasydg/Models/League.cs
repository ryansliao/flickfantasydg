namespace fantasydg.Models
{
    public class League
    {
        public int LeagueId { get; set; }
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public int PlayerNumber {  get; set; }
        public ApplicationUser Creator { get; set; }

        public ICollection<LeagueMember> Members { get; set; }
        public ICollection<Team> Teams { get; set; }
    }
}
