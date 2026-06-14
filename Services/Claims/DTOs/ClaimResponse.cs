namespace Claims.DTOs
{
    public class ClaimResponse
    {
        public Guid ClaimId { get; set; }
        public Guid PolicyId { get; set; }
        public string Description { get; set; } = "";
        public string ClaimType { get; set; } = "";
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}
