namespace fantasydg.Models
{
    public class LeagueInvitation
    {
        public int LeagueInvitationId { get; set; }
        public int LeagueId { get; set; }
        public string UserId { get; set; }
        public DateTime SentAt { get; set; }

        public League League { get; set; }
        public ApplicationUser User { get; set; }
    }
}
