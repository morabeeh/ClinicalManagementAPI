using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace ClinicalManagementAPI.Utility.Mail
{
    public interface IMailHelper
    {
        void SendEmailInBackground(string[] recipients, string subject, string body);
        void SendEmailWithAttachmentInBackground(string[] recipients, string subject, string body, Stream pdfStream, string fileName);
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
            Task.Run(() => SendEmailAsync(recipients, subject, body));
        }

        public void SendEmailWithAttachmentInBackground(string[] recipients, string subject, string body, Stream pdfStream, string fileName)
        {
            Task.Run(() => SendEmailWithAttachmentAsync(recipients, subject, body, pdfStream, fileName));
        }

        private async Task SendEmailAsync(string[] recipients, string subject, string body)
        {
            var message = CreateEmailMessage(recipients, subject, body);

            await SendMessageAsync(message);
        }

        private async Task SendEmailWithAttachmentAsync(string[] recipients, string subject, string body, Stream pdfStream, string fileName)
        {
            var message = CreateEmailMessage(recipients, subject, body);

            // Reset stream position and attach PDF
            pdfStream.Position = 0;
            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(pdfStream, ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = fileName
            };
            var multipart = new Multipart("mixed") { message.Body, attachment };
            message.Body = multipart;

            await SendMessageAsync(message);
        }

        private MimeMessage CreateEmailMessage(string[] recipients, string subject, string body)
        {
            var email = _configuration["Email:emailAddress"];
            var message = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart("html") { Text = body }
            };

            // Add sender's email
            message.From.Add(new MailboxAddress("", email)); 

            // Add recipients' emails
            message.To.AddRange(recipients.Select(recipient => new MailboxAddress("", recipient)));

            return message;
        }


        private async Task SendMessageAsync(MimeMessage message)
        {
            var email = _configuration["Email:emailAddress"];
            var password = _configuration["Email:password"];
            var host = _configuration["Email:host"];
            var port = _configuration.GetValue<int>("Email:port");

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(email, password);
                await smtpClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }
        }
    }
}
