namespace ClaimFlow.DTOs;

public class QuoteResponse
{
    public Guid QuoteId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TripCost { get; set; }
    public int NumberOfTravelers { get; set; }
    public decimal PremiumAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
