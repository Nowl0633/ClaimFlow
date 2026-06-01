using ClaimFlow.Data;
using ClaimFlow.DTOs;
using ClaimFlow.Models;

namespace ClaimFlow.Services;

public class QuoteService : QuoteServiceInterface
{
    private readonly AppDbContext _db;

    public QuoteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<QuoteResponse> RequestQuote(QuoteRequest request, Guid customerId)
    {
        int days = (request.EndDate - request.StartDate).Days;
        decimal premium = CalculatePremium(request.TripCost, days, request.NumberOfTravelers);

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

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();

        return new QuoteResponse
        {
            QuoteId = quote.Id,
            Destination = quote.Destination,
            StartDate = quote.StartDate,
            EndDate = quote.EndDate,
            TripCost = quote.TripCost,
            NumberOfTravelers = quote.NumberOfTravelers,

            PremiumAmount = quote.PremiumAmount,
            Status = quote.Status,
            CreatedAt = quote.CreatedAt
        };
    }

    private decimal CalculatePremium(decimal tripCost, int days, int travelers)
    {
        // 2% of trip value + £1.50 per day per traveler
        decimal baseAmt = tripCost * 0.02m;
        decimal perDay = days * travelers * 1.50m;
        decimal total = baseAmt + perDay;

        return total < 20m ? 20m : total;
    }
}
