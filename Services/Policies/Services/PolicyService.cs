using ClaimFlow.Data;
using ClaimFlow.Models;
using ClaimFlow.Services;
using Microsoft.EntityFrameworkCore;
using Policies.DTOs;

namespace Policies.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService? _audit;
        private readonly IEmailService? _email;

        public PolicyService(AppDbContext context, IAuditService? audit = null, IEmailService? email = null)
        {
            _context = context;
            _audit = audit;
            _email = email;
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

            if (_audit != null)
                await _audit.LogAsync("PolicyCreated", "Policy", policy.Id, customerId);

            if (_email != null)
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer != null)
                    await _email.SendAsync(
                        customer.Email,
                        "Your policy is confirmed",
                        $"Hi {customer.FirstName},\n\nYour travel insurance policy has been issued. Here are the details:\n\n" +
                        $"Policy ID: {policy.Id}\n" +
                        $"Destination: {policy.Destination}\n" +
                        $"Travel dates: {policy.StartDate:dd MMM yyyy} – {policy.EndDate:dd MMM yyyy}\n" +
                        $"Premium paid: {policy.PremiumPaid:C}\n" +
                        $"Status: {policy.Status}\n\n" +
                        $"Keep this email as your proof of cover.\n\nClaimFlow"
                    );
            }

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

        public async Task<List<PolicyResponse>> GetMyPoliciesAsync(Guid customerId)
        {
            var policies = await _context.Policies
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<PolicyResponse>();
            foreach (var p in policies)
            {
                result.Add(new PolicyResponse
                {
                    PolicyId = p.Id,
                    Status = p.Status,
                    Destination = p.Destination,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PremiumPaid = p.PremiumPaid
                });
            }

            return result;
        }
    }
}
