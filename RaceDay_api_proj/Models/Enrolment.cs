namespace RaceDay_api_proj.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        public User? Participant { get; set; }

        public Event? Event { get; set; }

        public Category? Category { get; set; }
    }
}

