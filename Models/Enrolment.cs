
namespace RaceDayAPI.Models
{
    public class Enrolment
    {
        public string EnrolmentId { get; set; } = string.Empty;
        public string ParticipantId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
