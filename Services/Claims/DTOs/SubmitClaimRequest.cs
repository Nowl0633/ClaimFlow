using System.ComponentModel.DataAnnotations;

namespace Claims.DTOs
{
    public class SubmitClaimRequest
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public string Description { get; set; } = "";
        [Required]
        public string ClaimType { get; set; } = "";

        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
