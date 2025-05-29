namespace fantasydg.Models
{
    public class LeagueOwnershipTransfer
    {
        public int LeagueOwnershipTransferId { get; set; }

        public int LeagueId { get; set; }
        public League League { get; set; }

        public string NewOwnerId { get; set; }
        public ApplicationUser NewOwner { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
