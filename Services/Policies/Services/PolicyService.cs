using ClaimFlow.Data;
using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;
using Policies.DTOs;

namespace Policies.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly AppDbContext _context;

        public PolicyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyResponse> AcceptQuoteAsync(Guid customerId, Guid quoteId)
        {
            // check customerId too so a user cant accept someone elses quote
            var quote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.Id == quoteId && q.CustomerId == customerId);

            if (quote == null)
                throw new KeyNotFoundException("Quote not found.");

            if (quote.Status != "Pending")
                throw new InvalidOperationException("Quote has already been used.");

            // transaction so the quote update and policy insert either both happen or neither does
            using var transaction = await _context.Database.BeginTransactionAsync();

            quote.Status = "Accepted";

            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                QuoteId = quoteId,
                Destination = quote.Destination,
                StartDate = quote.StartDate,
                EndDate = quote.EndDate,
                PremiumPaid = quote.PremiumAmount,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new PolicyResponse
            {
                PolicyId = policy.Id,
                Status = policy.Status,
                Destination = policy.Destination,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PremiumPaid = policy.PremiumPaid
            };
        }
    }
}
