
namespace RaceDayAPI.Models
{
    public class Event
    {
        public string EventId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "Upcoming";
        public string OrganiserId { get; set; } = string.Empty;
    }
}
