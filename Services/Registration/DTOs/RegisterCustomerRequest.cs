using System.ComponentModel.DataAnnotations;

namespace Registration.DTOs
{
    public class RegisterCustomerRequest
    {
        [Required]
        public string FirstName { get; set; } = "";
        [Required]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; } = "";
    }
}
