using ClaimFlow.DTOs;
using ClaimFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClaimFlow.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly RegistrationServiceInterface _registrationService;

        public CustomerController(RegistrationServiceInterface registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Phone) || request.DateOfBirth == default)
            {
                return BadRequest("All fields are required");
            }

            var taken = await _registrationService.EmailExists(request.Email);
            if (taken)
                return BadRequest("Email already in use");

            var result = await _registrationService.Register(request);
            return CreatedAtAction(nameof(Register), result);
        }
    }
}
