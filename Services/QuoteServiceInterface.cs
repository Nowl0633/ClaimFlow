using ClaimFlow.DTOs;

namespace ClaimFlow.Services;

public interface QuoteServiceInterface
{
    Task<QuoteResponse> RequestQuote(QuoteRequest request, Guid customerId);
}
