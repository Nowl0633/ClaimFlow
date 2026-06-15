using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quotes.DTOs;
using Quotes.Services;

namespace Quotes.Controllers
{
    [ApiController]
    [Route("api/quotes")]
    [Authorize]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _service;

        public QuotesController(IQuoteService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] QuoteRequest request)
        {
            // NameIdentifier is the customer id we put in the token at login
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var quote = await _service.CreateQuoteAsync(customerId, request);
                return Ok(quote);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
