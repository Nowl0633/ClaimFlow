namespace Login.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = "";
    }
}
