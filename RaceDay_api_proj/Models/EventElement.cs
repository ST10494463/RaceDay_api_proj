namespace RaceDay_api_proj.Models
{
    public class EventElement
    {
        public int EventElementID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public string ElementName { get; set; } = string.Empty;

        public decimal? Distance { get; set; }

        public TimeSpan? StartTime { get; set; }

        public string? Description { get; set; }

        public Event? Event { get; set; }

        public Category? Category { get; set; }
    }
}
