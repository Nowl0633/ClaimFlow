using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Policies.DTOs;
using Policies.Services;

namespace Policies.Controllers
{
    [ApiController]
    [Route("api/policies")]
    [Authorize]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _service;

        public PoliciesController(IPolicyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPolicies()
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var policies = await _service.GetMyPoliciesAsync(customerId);
            return Ok(policies);
        }

        // this is what converts a quote into an actual policy
        [HttpPost]
        public async Task<IActionResult> AcceptQuote([FromBody] AcceptQuoteRequest request)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var policy = await _service.AcceptQuoteAsync(customerId, request.QuoteId);
                return Ok(policy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
