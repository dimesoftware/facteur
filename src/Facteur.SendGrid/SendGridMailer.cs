using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Facteur.SendGrid
{
    /// <summary>
    /// Mail component that uses SendGrid as the transport and RazorEngine as the content builder
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SendGridMailer : SendGridBaseMailer, IMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridMailer"/> class
        /// </summary>
        /// <param name="key">The SendGrid API key</param>
        public SendGridMailer(string key)
            : base(key)
        {
        }

        /// <summary>
        /// Sends the mail asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override async Task SendMailAsync(EmailRequest request) 
            => await base.SendMailAsync(request).ConfigureAwait(false);

        /// <summary>
        /// Sends the mail asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task SendMailAsync<T>(EmailRequest<T> request)
            where T : class
        {
            EmailComposer composer = new EmailComposer();
            EmailRequest mailRequest = composer
                .SetSubject(request.Subject)
                .SetBody(request.Body)
                .SetFrom(request.From)
                .SetTo(request.To?.ToArray())
                .SetCc(request.Cc?.ToArray())
                .SetBcc(request.Bcc?.ToArray())
                .Build();

            await base.SendMailAsync(mailRequest).ConfigureAwait(false);
        }
    }
}