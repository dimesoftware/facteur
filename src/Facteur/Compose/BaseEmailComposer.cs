using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facteur
{
    public abstract class BaseEmailComposer<T> where T : EmailRequest, new()
    {
        protected BaseEmailComposer()
        {
            Request = new T();
        }

        protected T Request { get; }

        public BaseEmailComposer<T> SetSubject(string subject)
        {
            Request.Subject = subject;
            return this;
        }

        public virtual BaseEmailComposer<T> SetBody(string body)
        {
            Request.Body = body;
            return this;
        }

        public BaseEmailComposer<T> SetFrom(Sender sender)
        {
            Request.From = sender;
            return this;
        }

        public BaseEmailComposer<T> SetFrom(string email, string name = null)
        {
            Request.From = new Sender(email, name);
            return this;
        }

        public BaseEmailComposer<T> SetTo(params string[] to)
        {
            Request.To = to;
            return this;
        }

        public BaseEmailComposer<T> SetCc(params string[] cc)
        {
            Request.Cc = cc;
            return this;
        }

        public BaseEmailComposer<T> SetBcc(params string[] bcc)
        {
            Request.Bcc = bcc;
            return this;
        }

        public BaseEmailComposer<T> Attach(Attachment attachment)
        {
            Request.Attachments.Add(attachment);
            return this;
        }

        public BaseEmailComposer<T> Attach(IEnumerable<Attachment> attachments)
        {
            Request.Attachments.AddRange(attachments);
            return this;
        }

        public virtual T Build()
        {
            Guard.ThrowIfNull(Request.From, nameof(Request.From));
            Guard.ThrowIfNullOrEmpty(Request.From.Email, nameof(Request.From.Email));
            Guard.ThrowIfNullOrEmpty(Request.Subject, nameof(Request.Subject));
            Guard.ThrowIfNullOrEmpty(Request.To, nameof(Request.To));

            return Request;
        }

        public virtual Task<T> BuildAsync()
            => Task.FromResult(Build());
    }
}