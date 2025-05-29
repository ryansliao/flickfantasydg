namespace fantasydg.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<League> LeaguesOwned { get; set; }
    public ICollection<Team> TeamsOwned { get; set; }
    public ICollection<LeagueMember> LeagueMemberships { get; set; }
}