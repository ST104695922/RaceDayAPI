
namespace RaceDayAPI.Models
{
    public class Result
    {
        public string ResultId { get; set; } = string.Empty;
        public string EnrolmentId { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
    }
}
