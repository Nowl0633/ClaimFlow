using Policies.DTOs;

namespace Policies.Services
{
    public interface IPolicyService
    {
        Task<PolicyResponse> AcceptQuoteAsync(Guid customerId, Guid quoteId);
        Task<List<PolicyResponse>> GetMyPoliciesAsync(Guid customerId);
    }
}
