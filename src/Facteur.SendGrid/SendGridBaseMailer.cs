using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Facteur.SendGrid
{
    /// <summary>
    /// Base class for sending mails using SendGrid
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class SendGridBaseMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridBaseMailer"/> class
        /// </summary>
        /// <param name="apiKey">The api key</param>
        protected SendGridBaseMailer(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "Api key cannot be null");

            ApiKey = apiKey;
        }

        protected string ApiKey { get; }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="request">The subject.</param>
        /// <returns></returns>
        public virtual async Task SendMailAsync(EmailRequest request)
        {
            SendGridClient client = new(ApiKey);
            EmailAddress sendFrom = request.From.ToEmailAddress();
            List<EmailAddress> sendTo = [.. request.To.Select(x => new EmailAddress(x))];
            SendGridMessage message = MailHelper
                .CreateSingleEmailToMultipleRecipients(sendFrom, sendTo, request.Subject, null, request.Body)
                .AddAttachments(request);

            IEnumerable<string> sendCc = request.Cc.Where(x => !request.To.Contains(x));
            foreach (string cc in sendCc)
                message.AddCc(cc);

            IEnumerable<string> sendBcc = request.Bcc.Where(x => !request.To.Contains(x) && !request.Cc.Contains(x));
            foreach (string bcc in sendBcc)
                message.AddBcc(bcc);

            Response res = await client.SendEmailAsync(message);
            if (res.IsSuccessStatusCode == false)
                throw new Exception($"Failed to send email via SendGrid. StatusCode: {res.StatusCode}");
        }
    }
}