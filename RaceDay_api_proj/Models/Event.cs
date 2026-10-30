namespace RaceDay_api_proj.Models
{
    public class Event
    {
        public int EventID { get; set; }

        public int OrganizerID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = "Upcoming";

        public decimal Distance { get; set; }

        public string EventType { get; set; } = "Run";

       
        public Organizer? Organizer { get; set; }

        public ICollection<EventElement> EventElements { get; set; }
            = new List<EventElement>();
    }
}
