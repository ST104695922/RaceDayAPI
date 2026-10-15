
namespace RaceDayAPI.Models
{
    public class Payment
    {
        public string PaymentId { get; set; } = string.Empty;
        public string EnrolmentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? TransactionRef { get; set; }
    }
}
