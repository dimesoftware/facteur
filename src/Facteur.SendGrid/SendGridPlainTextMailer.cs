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
    /// Represents a mail service using SendGrid
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SendGridPlainTextMailer : SendGridBaseMailer, IMailer
    {
        private readonly IEmailComposer _composer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridPlainTextMailer"/> class
        /// </summary>
        /// <param name="key">The SendGrid API key</param>
        public SendGridPlainTextMailer(string key, IEmailComposer composer = null) : base(key)
        {
            _composer = composer ?? new EmailComposer();
        }

        /// <summary>
        /// Sends out an email
        /// </summary>
        /// <param name="request">The subject</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        public override Task SendMailAsync(EmailRequest request)
        {
            SendGridClient client = new(ApiKey);
            EmailAddress sendFrom = request.From.ToEmailAddress();
            List<EmailAddress> sendTo = request.To.Select(x => new EmailAddress(x)).ToList();
            SendGridMessage message = MailHelper.CreateSingleEmailToMultipleRecipients(sendFrom, sendTo, request.Subject, request.Body, null);

            return client.SendEmailAsync(message);
        }

        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await SendMailAsync(await compose(_composer));
    }
}