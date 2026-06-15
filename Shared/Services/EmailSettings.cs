namespace ClaimFlow.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string FromAddress { get; set; } = "noreply@claimflow.com";
        public string FromName { get; set; } = "ClaimFlow";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
