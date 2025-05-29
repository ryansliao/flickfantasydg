using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class LeagueMember
    {
        public int LeagueId { get; set; }
        public League League { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
