
namespace RaceDayAPI.Models
{
    public class Category
    {
        public string CategoryId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
    }
}
