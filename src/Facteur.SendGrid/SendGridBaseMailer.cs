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
        /// Sends the mail asynchronous.
        /// </summary>
        /// <param name="request">The subject.</param>
        /// <returns></returns>
        public virtual Task SendMailAsync(EmailRequest request)
        {
            SendGridClient client = new SendGridClient(ApiKey);
            EmailAddress sendFrom = new EmailAddress(request.From);
            List<EmailAddress> sendTo = request.To.Select(x => new EmailAddress(x)).ToList();
            SendGridMessage message = MailHelper.CreateSingleEmailToMultipleRecipients(sendFrom, sendTo, request.Subject, null, request.Body);

            return client.SendEmailAsync(message);
        }
    }
}