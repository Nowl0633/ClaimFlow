using ClaimFlow.Data;
using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Policies.Services;

namespace ClaimFlow.Tests.Policies
{
    public class PolicyServiceTests
    {
        // InMemory doesnt support real transactions so we suppress the warning it throws
        private static AppDbContext CreateContext() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        private static async Task<(AppDbContext context, Guid customerId, Guid quoteId)> SetupQuote(string status = "Pending")
        {
            var context = CreateContext();
            var customerId = Guid.NewGuid();
            var quoteId = Guid.NewGuid();

            context.Quotes.Add(new Quote
            {
                Id = quoteId,
                CustomerId = customerId,
                Destination = "Japan",
                StartDate = new DateTime(2026, 8, 1),
                EndDate = new DateTime(2026, 8, 15),
                TripCost = 3000m,
                PremiumAmount = 102m,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            return (context, customerId, quoteId);
        }

        [Fact]
        public async Task AcceptQuote_Pending_CreatesActivePolicy()
        {
            var (context, customerId, quoteId) = await SetupQuote();
            var service = new PolicyService(context);

            var result = await service.AcceptQuoteAsync(customerId, quoteId);

            Assert.Equal("Active", result.Status);
            Assert.Equal("Japan", result.Destination);
            Assert.Equal(102m, result.PremiumPaid);
            Assert.NotEqual(Guid.Empty, result.PolicyId);
        }

        [Fact]
        public async Task AcceptQuote_MarksQuoteAsAccepted()
        {
            var (context, customerId, quoteId) = await SetupQuote();
            var service = new PolicyService(context);

            await service.AcceptQuoteAsync(customerId, quoteId);

            var quote = await context.Quotes.FindAsync(quoteId);
            Assert.Equal("Accepted", quote!.Status);
        }

        [Fact]
        public async Task AcceptQuote_AlreadyAccepted_Throws()
        {
            var (context, customerId, quoteId) = await SetupQuote(status: "Accepted");
            var service = new PolicyService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AcceptQuoteAsync(customerId, quoteId));
        }

        [Fact]
        public async Task AcceptQuote_NotFound_Throws()
        {
            var service = new PolicyService(CreateContext());

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.AcceptQuoteAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task AcceptQuote_WrongCustomer_Throws()
        {
            var (context, _, quoteId) = await SetupQuote();
            var service = new PolicyService(context);

            // shouldnt be able to accept someone elses quote
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.AcceptQuoteAsync(Guid.NewGuid(), quoteId));
        }
    }
}
