using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Facteur.SendGrid
{
    [ExcludeFromCodeCoverage]
    public class SendGridMailer : SendGridBaseMailer, IMailer
    {
        public SendGridMailer(string key)
            : base(key)
        {
        }

        public override async Task SendMailAsync(EmailRequest request) 
            => await base.SendMailAsync(request);

        public async Task SendMailAsync<T>(EmailRequest<T> request)
            where T : class
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
    }
}