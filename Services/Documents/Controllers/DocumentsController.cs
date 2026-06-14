using System.Security.Claims;
using Documents.DTOs;
using Documents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Documents.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentsService _svc;

        public DocumentsController(IDocumentsService svc)
        {
            _svc = svc;
        }

        [HttpPost("{claimId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(Guid claimId, IFormFile file)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _svc.UploadAsync(customerId, claimId, file);
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

        [HttpGet("{claimId}")]
        public async Task<IActionResult> GetForClaim(Guid claimId)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var docs = await _svc.GetForClaimAsync(claimId, customerId);
                return Ok(docs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var custId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var (path, fileName) = await _svc.GetFileAsync(id, custId);
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                return File(bytes, "application/octet-stream", fileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { message = "File not found." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var custId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _svc.DeleteAsync(id, custId);
                return Ok(new { message = "Deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
