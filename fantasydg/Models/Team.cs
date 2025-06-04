using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models
{
    public class Team
    {
        public int TeamId { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Team name cannot exceed 20 characters.")]
        public string Name { get; set; }

        // The league this team belongs to
        public int LeagueId { get; set; }
        public League League { get; set; }

        // The user who owns this team
        public string OwnerId { get; set; }
        public ApplicationUser Owner { get; set; }

        // Players on the team (existing Player entities)
        public ICollection<TeamPlayer> TeamPlayers { get; set; }
        public ICollection<TeamPlayerTournament> TeamPlayerTournaments { get; set; }
    }
}
