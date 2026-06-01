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
        private readonly LoginServiceInterface _loginService;

        public CustomerController(RegistrationServiceInterface registrationService, LoginServiceInterface loginService)
        {
            _registrationService = registrationService;
            _loginService = loginService;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var result = await _loginService.Login(request);
            if (result == null)
                return Unauthorized("Invalid email or password");

            return Ok(result);
        }
    }
}
