using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models
{
    public class TournamentInputView
    {
        [Required(ErrorMessage = "Tournament ID is required.")]
        public int TournamentId { get; set; }
    }
}