namespace ClaimFlow.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
