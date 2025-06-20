using System.ComponentModel.DataAnnotations;

namespace fantasydg.Models.ViewModels
{
    public class TournamentLockViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public bool IsLocked { get; set; }
    }
}