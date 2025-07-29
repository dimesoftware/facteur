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

        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await base.SendMailAsync(await compose(_composer.Reset()));
    }
}