using System.Threading.Tasks;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using EpicSolutions.Models;
using Newtonsoft.Json;
using System;

namespace EpicSolutions.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkId=532713
    public class EmailSender : IEmailSender
    {
        public EmailSender(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var msg = message?.Split("^");
            return Execute("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",
                new Contact
                {
                    Email = email,
                    Message = msg[1],
                    Subject = subject,
                    Name = msg[0]
                });
        }

        public static Task Execute(string apiKey, Contact contact)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(contact?.Email, contact.Name),
                Subject = contact.Subject,
                PlainTextContent = contact.Message,
                HtmlContent = contact.Message
            };
            msg.AddTo(new EmailAddress("contacto@epicsolutions.cl"));
            return client.SendEmailAsync(msg);
        }
    }
}
