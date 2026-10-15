
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // The role tells the API which account type is being used.
        [Required]
        public string Role { get; set; } = "Participant";
    }
}
