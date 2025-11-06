using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace CinemaApp.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("omar3000om30@gmail.com", "ojyh zskq ifyu vwha")
            };
            return client.SendMailAsync(
                new MailMessage(from:"omar3000om30@gmail.com",
                to:email,
                subject,
                htmlMessage
                )
                {
                    IsBodyHtml = true,
                });
        }
    }
}
