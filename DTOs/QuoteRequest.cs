namespace ClaimFlow.DTOs
{
    public class QuoteRequest
    {
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TripCost { get; set; }
        public int NumberOfTravelers { get; set; }
    }
}
