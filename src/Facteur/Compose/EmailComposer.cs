using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facteur
{
    public class EmailComposer : IEmailComposer
    {
        public EmailComposer()
        {
            Request = new EmailRequest();
        }

        protected EmailRequest Request { get; }

        public IEmailComposer SetSubject(string subject)
        {
            Request.Subject = subject;
            return this;
        }

        public virtual IEmailComposer SetBody(string body)
        {
            Request.Body = body;
            return this;
        }

        public IEmailComposer SetFrom(Sender sender)
        {
            Request.From = sender;
            return this;
        }

        public IEmailComposer SetFrom(string email, string name = null)
        {
            Request.From = new Sender(email, name);
            return this;
        }

        public IEmailComposer SetTo(params string[] to)
        {
            Request.To = to;
            return this;
        }

        public IEmailComposer SetCc(params string[] cc)
        {
            Request.Cc = cc;
            return this;
        }

        public IEmailComposer SetBcc(params string[] bcc)
        {
            Request.Bcc = bcc;
            return this;
        }

        public IEmailComposer Attach(Attachment attachment)
        {
            Request.Attachments.Add(attachment);
            return this;
        }

        public IEmailComposer Attach(IEnumerable<Attachment> attachments)
        {
            Request.Attachments.AddRange(attachments);
            return this;
        }

        public virtual EmailRequest Build()
        {
            Guard.ThrowIfNull(Request.From, nameof(Request.From));
            Guard.ThrowIfNullOrEmpty(Request.From.Email, nameof(Request.From.Email));
            Guard.ThrowIfNullOrEmpty(Request.Subject, nameof(Request.Subject));
            Guard.ThrowIfNullOrEmpty(Request.To, nameof(Request.To));

            return Request;
        }

        public virtual Task<EmailRequest> BuildAsync()
            => Task.FromResult(Build());
    }
}