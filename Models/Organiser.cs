
using System.Text.Json.Serialization;

namespace RaceDayAPI.Models
{
    public class Organiser
    {
        public string OrganiserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
