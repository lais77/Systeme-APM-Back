using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace APM.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _config["EmailSettings:FromName"],
                _config["EmailSettings:FromEmail"]
            ));

            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _config["EmailSettings:SmtpHost"],
                int.Parse(_config["EmailSettings:SmtpPort"]!),
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _config["EmailSettings:SmtpUser"],
                _config["EmailSettings:SmtpPassword"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendToMultipleAsync(List<(string email, string name)> recipients, string subject, string htmlBody)
        {
            foreach (var (email, name) in recipients)
            {
                await SendEmailAsync(email, name, subject, htmlBody);
            }
        }
    }
}