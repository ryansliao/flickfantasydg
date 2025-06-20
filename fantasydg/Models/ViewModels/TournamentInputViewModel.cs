using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models.ViewModels
{
    public class TournamentInputViewModel
    {
        [Required(ErrorMessage = "Tournament ID is required.")]
        public int TournamentId { get; set; }
    }
}