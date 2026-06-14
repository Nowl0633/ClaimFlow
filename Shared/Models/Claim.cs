using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Models
{
    public class Claim
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid PolicyId { get; set; }
        public string Description { get; set; } = "";
        public string ClaimType { get; set; } = "";

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public string Status { get; set; } = "Submitted";
        public DateTime SubmittedAt { get; set; }
    }
}
