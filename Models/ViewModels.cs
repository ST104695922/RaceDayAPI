using System.ComponentModel.DataAnnotations;
using RaceDayMvc.Models.Api;

namespace RaceDayMvc.Models
{
    // Data shown on the error page.
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string Message { get; set; } = "Something went wrong. Please try again.";
    }

    // The login form.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Participant";

        public string? ReturnUrl { get; set; }
    }

    // The registration form.
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a password.")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "I am registering as")]
        public string Role { get; set; } = "Participant";
    }

    // The participant home page: filters plus the matching events.
    public class EventBrowseViewModel
    {
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public string? Distance { get; set; }
        public List<EventDto> Events { get; set; } = new();
    }

    // The event details page for participants.
    public class EventDetailsViewModel
    {
        public EventDto Event { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();

        // Category IDs the participant has already entered.
        public HashSet<string> EnrolledCategoryIds { get; set; } = new();

        public bool IsOpen => Event.Status == "Upcoming" && Event.EventDate.Date >= DateTime.Today;
    }

    // The organiser dashboard.
    public class DashboardViewModel
    {
        public List<OrganiserEventDto> Events { get; set; } = new();
        public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);

        public List<OrganiserEventDto> UpcomingEvents => Events
            .Where(e => e.Status == "Upcoming" && e.EventDate.Date >= DateTime.Today)
            .OrderBy(e => e.EventDate)
            .ToList();
    }

    // The create / edit event form.
    public class EventFormViewModel
    {
        public string? EventId { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "The description can be up to 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event date")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(100, MinimumLength = 2)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 9999.99, ErrorMessage = "Distance must be greater than zero.")]
        [Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Road Race";

        public string Status { get; set; } = "Upcoming";

        [Display(Name = "Banner image")]
        public IFormFile? Banner { get; set; }

        public string? ExistingBannerUrl { get; set; }
    }

    // The category management page.
    public class CategoriesViewModel
    {
        public EventDto Event { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public CategoryFormViewModel NewCategory { get; set; } = new();
    }

    // The add category form.
    public class CategoryFormViewModel
    {
        public string EventId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(50)]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the distance.")]
        [Range(0.01, 999.99, ErrorMessage = "Distance must be greater than zero.")]
        [Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }

        [Required(ErrorMessage = "Please enter the entry fee.")]
        [Range(0, 999999.99, ErrorMessage = "The entry fee cannot be negative.")]
        [Display(Name = "Entry fee (R)")]
        public decimal? EntryFee { get; set; }
    }

    // The enrolments and results pages for one event.
    public class EventEnrolmentsViewModel
    {
        public EventDto Event { get; set; } = new();
        public List<EnrolmentDto> Enrolments { get; set; } = new();
    }

    // The profile page.
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}