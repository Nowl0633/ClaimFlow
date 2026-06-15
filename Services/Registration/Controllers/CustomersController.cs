using Microsoft.AspNetCore.Mvc;
using Registration.DTOs;
using Registration.Services;

namespace Registration.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IRegistrationService _service;

        public CustomersController(IRegistrationService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
        {
            try
            {
                var result = await _service.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // 409 rather than 400 - the request was valid, email is just taken
                return Conflict(new { message = ex.Message });
            }
        }

        // no [Authorize] here - user might be locked out and cant log in to reset their password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _service.ResetPasswordAsync(request);
                return Ok(new { message = "Password updated." });

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
