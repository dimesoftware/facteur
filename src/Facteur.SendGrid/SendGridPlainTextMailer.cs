using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Facteur.SendGrid
{
    /// <summary>
    /// Represents a mail service using SendGrid
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SendGridPlainTextMailer : SendGridBaseMailer, IMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridPlainTextMailer"/> class
        /// </summary>
        /// <param name="key">The SendGrid API key</param>
        public SendGridPlainTextMailer(string key) : base(key)
        {
        }

        /// <summary>
        /// Sends out an e-mail
        /// </summary>
        /// <param name="request">The subject</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        public override async Task SendMailAsync(EmailRequest request)
        {
            SendGridClient client = new SendGridClient(ApiKey);
            EmailAddress sendFrom = new EmailAddress(request.From);
            List<EmailAddress> sendTo = request.To.Select(x => new EmailAddress(x)).ToList();
            SendGridMessage message = MailHelper.CreateSingleEmailToMultipleRecipients(sendFrom, sendTo, request.Subject, null, request.Body);

            // Send the email.
            Response response = await client.SendEmailAsync(message).ConfigureAwait(false);
        }

        public async Task SendMailAsync<T>(EmailRequest<T> request) where T : class
        {
            SendGridClient client = new SendGridClient(ApiKey);
            EmailAddress sendFrom = new EmailAddress(request.From);
            List<EmailAddress> sendTo = request.To.Select(x => new EmailAddress(x)).ToList();
            SendGridMessage message = MailHelper.CreateSingleEmailToMultipleRecipients(sendFrom, sendTo, request.Subject, null, request.Body);

            // Send the email.
            Response response = await client.SendEmailAsync(message).ConfigureAwait(false);
        }
    }
}