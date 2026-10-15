using System.Text.Json.Serialization;

namespace RaceDayAPI.Models
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Participant";
    }
}