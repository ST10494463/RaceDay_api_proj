namespace RaceDay_api_proj.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public int? EventElementID { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Active";

        public Participant? Participant { get; set; }

        public Event? Event { get; set; }

        public Category? Category { get; set; }

        public EventElement? EventElement { get; set; }
    }
}

