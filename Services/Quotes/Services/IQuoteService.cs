using Quotes.DTOs;

namespace Quotes.Services
{
    public interface IQuoteService
    {
        Task<QuoteResponse> CreateQuoteAsync(Guid customerId, QuoteRequest request);
    }
}
