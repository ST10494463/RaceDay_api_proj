namespace RaceDay_api_proj.Models
{
    public class Organizer
    {
        public int OrganizerID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
