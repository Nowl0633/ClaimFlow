using ClaimFlow.Data;
using Microsoft.EntityFrameworkCore;
using Quotes.DTOs;
using Quotes.Services;

namespace ClaimFlow.Tests.Quotes
{
    public class QuoteServiceTests
    {
        private static AppDbContext CreateContext() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task CreateQuote_ValidTrip_ReturnsCorrectPremium()
        {
            var service = new QuoteService(CreateContext());

            var result = await service.CreateQuoteAsync(Guid.NewGuid(), new QuoteRequest
            {
                Destination = "Japan",
                StartDate = new DateTime(2026, 8, 1),
                EndDate = new DateTime(2026, 8, 15),
                TripCost = 3000m,
                NumberOfTravelers = 2
            });

            // 2% of 3000 = 60, plus 1.50 x 14 days x 2 people = 42, total = 102
            Assert.Equal(102m, result.PremiumAmount);
            Assert.Equal("Pending", result.Status);
        }

        [Fact]
        public async Task CreateQuote_CheapTrip_ReturnsMinimum()
        {
            var service = new QuoteService(CreateContext());

            var result = await service.CreateQuoteAsync(Guid.NewGuid(), new QuoteRequest
            {
                Destination = "France",
                StartDate = new DateTime(2026, 7, 1),
                EndDate = new DateTime(2026, 7, 2),
                TripCost = 100m,
                NumberOfTravelers = 1
            });

            Assert.Equal(20m, result.PremiumAmount);
        }

        [Fact]
        public async Task CreateQuote_EndBeforeStart_Throws()
        {
            var service = new QuoteService(CreateContext());

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateQuoteAsync(Guid.NewGuid(), new QuoteRequest
                {
                    Destination = "Spain",
                    StartDate = new DateTime(2026, 8, 15),
                    EndDate = new DateTime(2026, 8, 1),
                    TripCost = 1000m,
                    NumberOfTravelers = 1
                }));
        }

        [Fact]
        public async Task CreateQuote_SavesToDB()
        {
            var context = CreateContext();
            var service = new QuoteService(context);
            var customerId = Guid.NewGuid();

            var result = await service.CreateQuoteAsync(customerId, new QuoteRequest
            {
                Destination = "Italy",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 8),
                TripCost = 2000m,
                NumberOfTravelers = 1
            });

            var saved = await context.Quotes.FindAsync(result.QuoteId);
            Assert.NotNull(saved);
            Assert.Equal(customerId, saved.CustomerId);
        }
    }
}
