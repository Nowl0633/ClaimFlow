using ClaimFlow.Data;
using ClaimFlow.Models;
using Quotes.DTOs;

namespace Quotes.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly AppDbContext _context;

        public QuoteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuoteResponse> CreateQuoteAsync(Guid customerId, QuoteRequest request)
        {
            if (request.EndDate <= request.StartDate)
                throw new ArgumentException("Return date must be after departure date.");

            int days = (request.EndDate - request.StartDate).Days;

            // formula: 2% of trip cost + £1.50 per day per traveller, min £20
            decimal basePremium = request.TripCost * 0.02m;
            decimal dailyCharge = 1.50m * days * request.NumberOfTravelers;
            decimal total = basePremium + dailyCharge;
            decimal premium = Math.Round(Math.Max(20m, total), 2);

            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Destination = request.Destination,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TripCost = request.TripCost,
                NumberOfTravelers = request.NumberOfTravelers,
                PremiumAmount = premium,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return new QuoteResponse
            {
                QuoteId = quote.Id,
                Destination = quote.Destination,
                StartDate = quote.StartDate,
                EndDate = quote.EndDate,
                TripCost = quote.TripCost,
                NumberOfTravelers = quote.NumberOfTravelers,
                PremiumAmount = quote.PremiumAmount,
                Status = quote.Status
            };
        }
    }
}
