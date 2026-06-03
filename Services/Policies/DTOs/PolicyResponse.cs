namespace Policies.DTOs
{
    public class PolicyResponse
    {
        public Guid PolicyId { get; set; }
        public string Status { get; set; } = "";
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumPaid { get; set; }
    }
}
