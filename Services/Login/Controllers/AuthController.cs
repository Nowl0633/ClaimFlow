using Login.DTOs;
using Login.Services;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _service;

        public AuthController(ILoginService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _service.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                // dont tell them which bit was wrong
                return Unauthorized(new { message = "Invalid email or password." });
            }
        }
    }
}
