using System.ComponentModel.DataAnnotations;

namespace Registration.DTOs
{
    public class RegisterCustomerRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = "";
    }
}
