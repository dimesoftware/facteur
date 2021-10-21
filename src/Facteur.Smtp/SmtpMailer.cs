using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Facteur.Smtp
{
    /// <summary>
    /// Mail component that uses SMTP as the transport and RazorEngine as the content builder
    /// </summary>
    public class SmtpMailer : IMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailer"/> class
        /// </summary>
        /// <param name="smtpCredentials">The SMTP credentials</param>
        public SmtpMailer(SmtpCredentials smtpCredentials)
        {
            Credentials = smtpCredentials;
        }

        protected SmtpCredentials Credentials { get; }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task SendMailAsync(EmailRequest request)
        {
            using SmtpClient smtpClient = new(Credentials.Host)
            {
                Port = Credentials.Port,
                Credentials = new NetworkCredential(Credentials.Email, Credentials.Password),
                EnableSsl = Credentials.EnableSsl
            };

            if (Credentials.UseDefaultCredentials)
                smtpClient.UseDefaultCredentials = Credentials.UseDefaultCredentials;

            using MailMessage msg = new();
            msg.Subject = request.Subject;
            msg.Body = request.Body;
            msg.IsBodyHtml = true;
            msg.From = request.From.ToMailAddress();

            if (request.To != null && request.To.Any())
                msg.To.Add(string.Join(",", request.To));

            if (request.Cc != null && request.Cc.Any())
                msg.CC.Add(string.Join(",", request.Cc));

            if (request.Bcc != null && request.Bcc.Any())
                msg.Bcc.Add(string.Join(",", request.Bcc));

            foreach (Attachment attachment in request.Attachments)
                msg.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(attachment.ContentBytes), attachment.Name));

            await smtpClient.SendMailAsync(msg);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task SendMailAsync<T>(EmailRequest<T> request) where T : class
            => await SendMailAsync((EmailRequest)request).ConfigureAwait(false);
    }
}