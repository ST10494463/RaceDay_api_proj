namespace RaceDay_api_proj.Models
{
    public class Result
    {
        public int ResultID { get; set; }

        public int ParticipantID { get; set; }

        public int EventElementID { get; set; }

        public int EnrolmentID { get; set; }

        public int Position { get; set; }

        public int FinishPosition { get; set; }

        public TimeSpan FinishTime { get; set; }

        public string ResultStatus { get; set; } = string.Empty;
    }
}
