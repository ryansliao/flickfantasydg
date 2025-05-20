using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models
{
    public class TournamentInputView
    {
        [Required(ErrorMessage = "Tournament ID is required.")]
        public int TournamentId { get; set; }
        [Required(ErrorMessage = "Division is required.")]
        public string? Division { get; set; }
        public List<SelectListItem> DivisionOptions { get; set; } = new();
    }
}
