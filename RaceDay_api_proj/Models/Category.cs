namespace RaceDay_api_proj.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public int EventID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? MinimumAge { get; set; }

        public int? MaximumAge { get; set; }

        public decimal EntryFee { get; set; } = 0.00m;

        public Event? Event { get; set; }

        public ICollection<EventElement> EventElements { get; set; }
            = new List<EventElement>();
    }
}
