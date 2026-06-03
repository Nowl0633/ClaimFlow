using System.ComponentModel.DataAnnotations;

namespace Policies.DTOs
{
    public class AcceptQuoteRequest
    {
        [Required]
        public Guid QuoteId { get; set; }
    }
}
