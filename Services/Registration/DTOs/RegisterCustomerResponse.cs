namespace Registration.DTOs
{
    public class RegisterCustomerResponse
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; } = "";
        public string Message { get; set; } = string.Empty;
    }
}
