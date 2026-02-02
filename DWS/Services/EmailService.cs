using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DWS.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailSettings = _config.GetSection("EmailSettings");
            
            var client = new SmtpClient(mailSettings["Host"], int.Parse(mailSettings["Port"]))
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailSettings["Mail"], mailSettings["Password"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(mailSettings["Mail"], "MedIQ Support"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
