using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facteur
{
    public class EmailComposer : IEmailComposer
    {
        private readonly ITemplateCompiler _compiler;
        private readonly ITemplateProvider _provider;
        private readonly ITemplateResolver _resolver;

        public EmailComposer()
        {
            Request = new EmailRequest();
        }

        public EmailComposer(ITemplateCompiler compiler, ITemplateProvider provider, ITemplateResolver resolver)
            : this()
        {
            _compiler = compiler;
            _provider = provider;
            _resolver = resolver;
        }

        protected EmailRequest Request { get; private set; }

        /// <summary>
        /// Resets the EmailRequest to a fresh instance to prevent state lingering between email sends
        /// </summary>
        public IEmailComposer Reset()
        {
            Request = new EmailRequest();
            return this;
        }

        public IEmailComposer Subject(string subject)
        {
            Request.Subject = subject;
            return this;
        }

        public IEmailComposer SetSubject(string subject)
            => Subject(subject);

        public virtual IEmailComposer Body(string body)
        {
            Request.Body = body;
            return this;
        }

        public virtual IEmailComposer SetBody(string body)
            => Body(body);

        public IEmailComposer From(Sender sender)
        {
            Request.From = sender;
            return this;
        }

        public IEmailComposer SetFrom(Sender sender)
            => From(sender);

        public IEmailComposer From(string email, string name = null)
        {
            Request.From = new Sender(email, name);
            return this;
        }

        public IEmailComposer SetFrom(string email, string name = null)
            => From(email, name);

        public IEmailComposer To(params string[] to)
        {
            Request.To = to;
            return this;
        }

        public IEmailComposer SetTo(params string[] to)
            => To(to);

        public IEmailComposer Cc(params string[] cc)
        {
            Request.Cc = cc;
            return this;
        }

        public IEmailComposer SetCc(params string[] cc)
            => Cc(cc);

        public IEmailComposer Bcc(params string[] bcc)
        {
            Request.Bcc = bcc;
            return this;
        }

        public IEmailComposer SetBcc(params string[] bcc)
            => Bcc(bcc);

        public IEmailComposer Attach(Attachment attachment)
        {
            Request.Attachments.Add(attachment);
            return this;
        }

        public IEmailComposer Attach(string name, byte[] contents)
            => Attach(new Attachment(name, contents));

        public IEmailComposer Attach(IEnumerable<Attachment> attachments)
        {
            Request.Attachments.AddRange(attachments);
            return this;
        }

        public IEmailComposer Attach(params Attachment[] attachments)
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

            EmailRequest result = Request;
            Reset();
            return result;
        }

        public virtual Task<EmailRequest> BuildAsync()
            => Task.FromResult(Build());

        public async Task<EmailRequest> BuildAsync<T>(T model)
        {
            if (_compiler == null || _resolver == null || _provider == null)
            {
                EmailRequest result = Request;
                Reset(); // Reset for next use
                return result;
            }

            string templateName = _resolver?.Resolve(model);
            string templateContent = await _provider?.GetTemplate(templateName);
            string compiledBody = await _compiler?.CompileBody(model, templateContent);

            Request.Body = compiledBody;

            return Build();
        }
    }
}