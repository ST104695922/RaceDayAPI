namespace RaceDayMvc.Models.Api
{
    // These classes only describe the JSON the RaceDay API sends back.
    // They contain no business rules; the API owns all of the logic.

    // Details of the user who just logged in.
    public class LoginResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // A single event.
    public class EventDto
    {
        public string EventId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public string OrganiserId { get; set; } = string.Empty;
    }

    // An organiser's event together with its totals (used by the dashboard).
    public class OrganiserEventDto : EventDto
    {
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
    }

    // A category (age or distance group) within an event.
    public class CategoryDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
    }

    // An enrolment joined with its participant, event, category and result.
    public class EnrolmentDto
    {
        public string EnrolmentId { get; set; } = string.Empty;
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventStatus { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ResultId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int FinishersCount { get; set; }
    }

    // The logged-in user's profile.
    public class ProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}