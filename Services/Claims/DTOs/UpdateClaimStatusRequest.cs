using System.ComponentModel.DataAnnotations;

namespace Claims.DTOs
{
    public class UpdateClaimStatusRequest
    {
        [Required]
        public string Status { get; set; } = "";
    }
}
