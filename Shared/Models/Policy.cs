using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Models
{
    public class Policy
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Precision(18, 2)]
        public decimal PremiumPaid { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }
}
