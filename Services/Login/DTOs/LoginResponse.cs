namespace Login.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
