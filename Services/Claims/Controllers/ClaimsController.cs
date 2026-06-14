using System.Security.Claims;
using Claims.DTOs;
using Claims.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    [ApiController]
    [Route("api/claims")]
    [Authorize]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsService _service;

        public ClaimsController(IClaimsService service)
        {
            _service = service;
        }

        // pulled this out so i dont have to write the same line in every action
        private Guid GetCustId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitClaimRequest request)
        {
            var custId = GetCustId();

            try
            {
                var result = await _service.SubmitAsync(custId, request);
                return Ok(result);
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

        [HttpGet]
        public async Task<IActionResult> GetMyClaims()
        {
            var custId = GetCustId();
            var claims = await _service.GetMyClaimsAsync(custId);
            return Ok(claims);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateClaimStatusRequest request)
        {
            var custId = GetCustId();

            try
            {
                var result = await _service.UpdateStatusAsync(id, custId, request.Status);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
