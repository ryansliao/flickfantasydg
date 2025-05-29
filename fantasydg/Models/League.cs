using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models
{
    public class League
    {
        public int LeagueId { get; set; }

        [Required]
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public int PlayerNumber {  get; set; }

        [ValidateNever]
        public ApplicationUser Owner { get; set; }
        [ValidateNever]
        public ICollection<LeagueMember> Members { get; set; }
        [ValidateNever]
        public ICollection<Team> Teams { get; set; }
    }
}
