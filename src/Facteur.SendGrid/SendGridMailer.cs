using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Facteur.SendGrid
{
    [ExcludeFromCodeCoverage]
    public class SendGridMailer : SendGridBaseMailer, IMailer
    {
        private readonly IEmailComposer _composer;

        public SendGridMailer(string key, IEmailComposer composer = null)
            : base(key)
        {
            _composer = composer ?? new EmailComposer();
        }

        public override async Task SendMailAsync(EmailRequest request)
        {
            EmailComposer composer = new();
            EmailRequest mailRequest = composer
                .SetSubject(request.Subject)
                .SetBody(request.Body)
                .SetFrom(request.From)
                .SetTo(request.To?.ToArray())
                .SetCc(request.Cc?.ToArray())
                .SetBcc(request.Bcc?.ToArray())
                .Attach(request.Attachments)
                .Build();

            await base.SendMailAsync(mailRequest);
        }

        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await SendMailAsync(await compose(_composer));
    }
}