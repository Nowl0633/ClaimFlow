using System.ComponentModel.DataAnnotations;

namespace Quotes.DTOs
{
    public class QuoteRequest
    {
        [Required]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, double.MaxValue)]
        public decimal TripCost { get; set; }
        [Range(1, 20)]
        public int NumberOfTravelers { get; set; }
    }
}
