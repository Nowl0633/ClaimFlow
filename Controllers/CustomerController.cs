using ClaimFlow.DTOs;
using ClaimFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClaimFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public CustomerController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCustomerRequest request)
        {
            bool taken = await _registrationService.EmailExists(request.Email);
            if (taken)
                return BadRequest("Email already in use");

            var result = await _registrationService.Register(request);
            return CreatedAtAction(nameof(Register), result);
        }
    }
}