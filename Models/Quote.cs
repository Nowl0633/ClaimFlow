using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Models
{
    public class Quote
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Precision(18, 2)]
        public decimal TripCost { get; set; }
        public int NumberOfTravelers { get; set; }
        [Precision(18, 2)]
        public decimal PremiumAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
