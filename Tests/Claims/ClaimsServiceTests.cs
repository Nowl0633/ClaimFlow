using ClaimFlow.Data;
using ClaimFlow.Models;
using Claims.DTOs;
using Claims.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClaimFlow.Tests.Claims
{
    public class ClaimsServiceTests
    {
        private static AppDbContext GetDb() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        private static async Task<(AppDbContext db, Guid customerId, Guid policyId)> SeedPolicy(string status = "Active")
        {
            var db = GetDb();
            var customerId = Guid.NewGuid();
            var policyId = Guid.NewGuid();

            db.Policies.Add(new Policy
            {
                Id = policyId,
                CustomerId = customerId,
                QuoteId = Guid.NewGuid(),
                Destination = "France",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 14),
                PremiumPaid = 85m,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return (db, customerId, policyId);
        }

        [Fact]
        public async Task SubmitClaim_ActivePolicy_ReturnsSubmitted()
        {
            var (db, customerId, policyId) = await SeedPolicy();
            var svc = new ClaimsService(db);

            var result = await svc.SubmitAsync(customerId, new SubmitClaimRequest
            {
                PolicyId = policyId,
                Description = "Lost my luggage at CDG airport",
                ClaimType = "Baggage",
                Amount = 450m
            });

            Assert.Equal("Submitted", result.Status);
            Assert.Equal("Baggage", result.ClaimType);
            Assert.Equal(450m, result.Amount);
            Assert.NotEqual(Guid.Empty, result.ClaimId);
        }

        [Fact]
        public async Task SubmitClaim_PolicyNotFound_Throws()
        {
            var svc = new ClaimsService(GetDb());

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.SubmitAsync(Guid.NewGuid(), new SubmitClaimRequest
                {
                    PolicyId = Guid.NewGuid(),
                    Description = "test",
                    ClaimType = "Medical",
                    Amount = 100m
                }));
        }

        [Fact]
        public async Task SubmitClaim_WrongCustomer_Throws()
        {
            var (db, _, policyId) = await SeedPolicy();
            var svc = new ClaimsService(db);

            // someone else's policy, should be blocked
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.SubmitAsync(Guid.NewGuid(), new SubmitClaimRequest
                {
                    PolicyId = policyId,
                    Description = "test",
                    ClaimType = "Medical",
                    Amount = 200m
                }));
        }

        [Fact]
        public async Task SubmitClaim_InactivePolicy_Throws()
        {
            var (db, customerId, policyId) = await SeedPolicy(status: "Expired");
            var svc = new ClaimsService(db);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.SubmitAsync(customerId, new SubmitClaimRequest
                {
                    PolicyId = policyId,
                    Description = "flight cancelled",
                    ClaimType = "Cancellation",
                    Amount = 1200m
                }));
        }

        [Fact]
        public async Task GetMyClaims_ReturnsMineOnly()
        {
            var (db, customerId, policyId) = await SeedPolicy();
            var svc = new ClaimsService(db);

            // 2 for our customer, 1 for someone else
            db.Claims.Add(new Claim { Id = Guid.NewGuid(), CustomerId = customerId, PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow });
            db.Claims.Add(new Claim { Id = Guid.NewGuid(), CustomerId = customerId, PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow });
            db.Claims.Add(new Claim { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var results = await svc.GetMyClaimsAsync(customerId);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task GetMyClaims_OrderedNewestFirst()
        {
            var (db, customerId, policyId) = await SeedPolicy();
            var svc = new ClaimsService(db);

            db.Claims.Add(new Claim { Id = Guid.NewGuid(), CustomerId = customerId, PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow.AddDays(-5), Description = "old one" });
            db.Claims.Add(new Claim { Id = Guid.NewGuid(), CustomerId = customerId, PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow, Description = "new one" });
            await db.SaveChangesAsync();

            var results = await svc.GetMyClaimsAsync(customerId);

            Assert.Equal("new one", results[0].Description);
            Assert.Equal("old one", results[1].Description);
        }

        [Fact]
        public async Task UpdateStatus_Approve_ChangesStatus()
        {
            var (db, customerId, policyId) = await SeedPolicy();
            var svc = new ClaimsService(db);

            var claimId = Guid.NewGuid();
            db.Claims.Add(new Claim { Id = claimId, CustomerId = customerId, PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await svc.UpdateStatusAsync(claimId, customerId, "Approved");
            Assert.Equal("Approved", result.Status);
        }

        [Fact]
        public async Task UpdateStatus_BadStatus_Throws()
        {
            var (db, customerId, policyId) = await SeedPolicy();
            var svc = new ClaimsService(db);

            var claimId = Guid.NewGuid();
            db.Claims.Add(new Claim { Id = claimId, CustomerId = customerId, PolicyId = policyId, Status = "Submitted", SubmittedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            // "Pending" isnt a valid status to set
            await Assert.ThrowsAsync<ArgumentException>(
                () => svc.UpdateStatusAsync(claimId, customerId, "Pending"));
        }

        [Fact]
        public async Task UpdateStatus_ClaimNotFound_Throws()
        {
            var svc = new ClaimsService(GetDb());

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.UpdateStatusAsync(Guid.NewGuid(), Guid.NewGuid(), "Approved"));
        }
    }
}
