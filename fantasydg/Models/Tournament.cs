namespace fantasydg.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Tournament
{
    [Key, Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    [Key, Column(Order = 1)]
    public string Division { get; set; } = null!;

    public DateTime Date { get; set; }
    public string? Name { get; set; }
    public string? Tier { get; set; }
    public int RoundNumber { get; set; }

    public ICollection<PlayerTournament> PlayerTournaments { get; set; }
    public ICollection<LeagueTournament> LeagueTournaments { get; set; }
}