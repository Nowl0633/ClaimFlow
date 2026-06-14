using ClaimFlow.Data;
using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;
using Claims.DTOs;

namespace Claims.Services
{
    public class ClaimsService : IClaimsService
    {
        private readonly AppDbContext _db;

        public ClaimsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ClaimResponse> SubmitAsync(Guid customerId, SubmitClaimRequest request)
        {
            // policy has to exist AND belong to this customer
            var policy = await _db.Policies
                .FirstOrDefaultAsync(p => p.Id == request.PolicyId && p.CustomerId == customerId);

            if (policy == null)
                throw new KeyNotFoundException("Policy not found.");

            if (policy.Status != "Active")
                throw new InvalidOperationException("Can only submit a claim against an active policy.");

            var claim = new Claim
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                PolicyId = request.PolicyId,
                Description = request.Description,
                ClaimType = request.ClaimType,
                Amount = request.Amount,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            };

            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();

            return ToDto(claim);
        }

        public async Task<List<ClaimResponse>> GetMyClaimsAsync(Guid customerId)
        {
            var claims = await _db.Claims
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            var list = new List<ClaimResponse>();
            foreach (var c in claims)
                list.Add(ToDto(c));

            return list;
        }

        public async Task<ClaimResponse> UpdateStatusAsync(Guid claimId, Guid customerId, string newStatus)
        {
            // only allow these three - Submitted isnt a valid transition target
            if (newStatus != "Approved" && newStatus != "Declined" && newStatus != "Settled")
                throw new ArgumentException($"{newStatus} is not a valid status.");

            var claim = await _db.Claims
                .FirstOrDefaultAsync(c => c.Id == claimId && c.CustomerId == customerId);

            if (claim == null)
                throw new KeyNotFoundException("Claim not found.");

            claim.Status = newStatus;
            await _db.SaveChangesAsync();

            return ToDto(claim);
        }

        private ClaimResponse ToDto(Claim c)
        {
            return new ClaimResponse
            {
                ClaimId = c.Id,
                PolicyId = c.PolicyId,
                Description = c.Description,
                ClaimType = c.ClaimType,
                Amount = c.Amount,
                Status = c.Status,
                SubmittedAt = c.SubmittedAt
            };
        }
    }
}
