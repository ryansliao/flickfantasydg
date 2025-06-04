using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fantasydg.Models
{
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PDGANumber { get; set; }
        public int ResultId { get; set; }
        public string? Name { get; set; }
        public ICollection<PlayerTournament> PlayerTournaments { get; set; }
        public ICollection<TeamPlayer> TeamPlayers { get; set; }
        public ICollection<TeamPlayerTournament> TeamPlayerTournaments { get; set; }
    }
}
