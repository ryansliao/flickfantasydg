namespace fantasydg.Models
{
    public class TournamentDropdownItem
    {
        public int id {  get; set; }
        internal object Division;

        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public int Rounds { get; set; }
        public int Id { get; internal set; }
    }
}
