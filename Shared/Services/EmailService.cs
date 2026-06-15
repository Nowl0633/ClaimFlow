using System.Net;
using System.Net.Mail;

namespace ClaimFlow.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            // if no smtp host is configured just log it - useful for local dev
            if (string.IsNullOrEmpty(_settings.SmtpHost))
            {
                Console.WriteLine($"[EMAIL] To: {to}");
                Console.WriteLine($"[EMAIL] Subject: {subject}");
                Console.WriteLine($"[EMAIL] {body}");
                return;
            }

            var msg = new MailMessage();
            msg.From = new MailAddress(_settings.FromAddress, _settings.FromName);
            msg.To.Add(to);
            msg.Subject = subject;
            msg.Body = body;

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

            await client.SendMailAsync(msg);
        }
    }
}
