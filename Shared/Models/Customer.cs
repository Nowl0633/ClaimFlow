namespace ClaimFlow.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
