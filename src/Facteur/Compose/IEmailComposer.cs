using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// Represents a type that is capable of constructing an email that is ready to be sent out.
    /// </summary>
    public interface IEmailComposer
    {
        IEmailComposer SetSubject(string subject);

        IEmailComposer SetBody(string body);

        IEmailComposer SetFrom(Sender sender);

        IEmailComposer SetFrom(string email, string name = null);

        IEmailComposer SetTo(params string[] to);

        IEmailComposer SetCc(params string[] cc);

        IEmailComposer SetBcc(params string[] bcc);

        IEmailComposer Attach(IEnumerable<Attachment> attachments);

        IEmailComposer Attach(Attachment attachment);

        EmailRequest Build();

        Task<EmailRequest> BuildAsync();

        Task<EmailRequest> BuildAsync<T>(T model);
    }
}