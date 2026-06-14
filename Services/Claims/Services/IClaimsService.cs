using Claims.DTOs;

namespace Claims.Services
{
    public interface IClaimsService
    {
        Task<ClaimResponse> SubmitAsync(Guid customerId, SubmitClaimRequest request);
        Task<List<ClaimResponse>> GetMyClaimsAsync(Guid customerId);
        Task<ClaimResponse> UpdateStatusAsync(Guid claimId, Guid customerId, string newStatus);
    }
}
