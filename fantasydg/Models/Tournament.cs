namespace fantasydg.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Tournament
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Division { get; set; } = null!;

    public DateTime Date { get; set; }
    public string? Name { get; set; }
    public int Rounds { get; set; }
    public double Weight { get; set; }

    public List<Round> RoundList { get; set; } = new();
}