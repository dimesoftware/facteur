using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Facteur.Resend
{
    [ExcludeFromCodeCoverage]
    public class ResendMailer : ResendBaseMailer, IMailer
    {
        private readonly IEmailComposer _composer;

        public ResendMailer(string key, IEmailComposer composer = null)
            : base(key)
        {
            _composer = composer ?? new EmailComposer();
        }

        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await base.SendMailAsync(await compose(_composer));
    }
}