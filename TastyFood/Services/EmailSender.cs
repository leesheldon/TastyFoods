using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TastyFood.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("", ""), // Email and mail box password from sender
                EnableSsl = true
            };

            MailMessage mailMessage = new MailMessage
            {
                // Email from sender
                From = new MailAddress("")
            };

            mailMessage.To.Add(email);
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = subject;

            client.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}
