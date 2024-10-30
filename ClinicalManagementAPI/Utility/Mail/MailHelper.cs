using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace ClinicalManagementAPI.Utility.Mail
{
    public interface IMailHelper
    {
        void SendEmailInBackground(string[] recipients, string subject, string body);
    }

    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmailInBackground(string[] recipients, string subject, string body)
        {
            // Run the email-sending logic as a background task
            Task.Run(async () =>
            {
                var email = _configuration.GetValue<string>("Email:emailAddress");
                var password = _configuration.GetValue<string>("Email:password");
                var host = _configuration.GetValue<string>("Email:host");
                var port = _configuration.GetValue<int>("Email:port");

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(email!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Enable HTML content
                };

                // Add each recipient to the message
                foreach (var recipient in recipients)
                {
                    message.To.Add(new MailAddress(recipient));
                }

                try
                {
                    await smtpClient.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            });
        }
    }
}
