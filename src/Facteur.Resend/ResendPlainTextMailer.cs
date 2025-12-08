using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Resend;

namespace Facteur.Resend
{
    /// <summary>
    /// Represents a mail service using Resend
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResendPlainTextMailer : ResendBaseMailer, IMailer
    {
        private readonly IEmailComposer _composer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResendPlainTextMailer"/> class
        /// </summary>
        /// <param name="key">The Resend API key</param>
        public ResendPlainTextMailer(string key, IEmailComposer composer = null) : base(key)
        {
            _composer = composer ?? new EmailComposer();
        }

        /// <summary>
        /// Sends out an email
        /// </summary>
        /// <param name="request">The email request</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        public override async Task SendMailAsync(EmailRequest request)
        {
            OptionsSnapshotWrapper<ResendClientOptions> options = new(new ResendClientOptions { ApiToken = ApiKey });
            HttpClient httpClient = new();
            ResendClient client = new(options, httpClient);

            EmailMessage message = new()
            {
                From = new() { Email = request.From.Email, DisplayName = request.From.Name },
                Subject = request.Subject,
                TextBody = request.Body
            };

            foreach (string to in request.To)
                message.To.Add(to);

            message.AddCc(request);
            message.AddBcc(request);
            message.AddAttachments(request);

            await client.EmailSendAsync(message);
        }

        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await SendMailAsync(await compose(_composer));
    }
}