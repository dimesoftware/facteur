using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// Represents a type that is capable of constructing an email that is ready to be sent out.
    /// </summary>
    public interface IEmailComposer
    {
        [Obsolete("Use Subject method instead")]
        IEmailComposer SetSubject(string subject);

        IEmailComposer Subject(string subject);

        [Obsolete("Use Body method instead")]
        IEmailComposer SetBody(string body);

        IEmailComposer Body(string body);

        [Obsolete("Use From method instead")]
        IEmailComposer SetFrom(Sender sender);

        IEmailComposer From(Sender sender);

        [Obsolete("Use From method instead")]
        IEmailComposer SetFrom(string email, string name = null);
        
        IEmailComposer From(string email, string name = null);

        [Obsolete("Use To method instead")]
        IEmailComposer SetTo(params string[] to);

        IEmailComposer To(params string[] to);

        [Obsolete("Use Cc method instead")]
        IEmailComposer SetCc(params string[] cc);

        IEmailComposer Cc(params string[] cc);

        [Obsolete("Use Bcc method instead")]
        IEmailComposer SetBcc(params string[] bcc);

        IEmailComposer Bcc(params string[] bcc);

        IEmailComposer Attach(IEnumerable<Attachment> attachments);

        IEmailComposer Attach(params Attachment[] attachments);

        IEmailComposer Attach(Attachment attachment);

        IEmailComposer Attach(string name, byte[] contents);

        EmailRequest Build();

        Task<EmailRequest> BuildAsync();

        Task<EmailRequest> BuildAsync<T>(T model);
    }
}