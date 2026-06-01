using System.Security.Claims;
using ClaimFlow.DTOs;
using ClaimFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimFlow.Controllers
{
    [ApiController]
    [Route("api/quotes")]
    [Authorize]
    public class QuoteController : ControllerBase
    {
        private readonly QuoteServiceInterface _quoteService;

        public QuoteController(QuoteServiceInterface quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpPost]
        public async Task<IActionResult> RequestQuote(QuoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Destination) ||
                request.StartDate == default || request.EndDate == default ||
                request.TripCost <= 0 || request.NumberOfTravelers <= 0)
            {
                return BadRequest("All fields are required and must be valid");
            }

            if (request.EndDate <= request.StartDate)
                return BadRequest("End date must be after start date");

            var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customerId = Guid.Parse(customerIdStr!);

            var result = await _quoteService.RequestQuote(request, customerId);
            return Ok(result);
        }
    }
}
