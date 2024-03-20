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

            return Request;
        }

        public virtual Task<EmailRequest> BuildAsync()
            => Task.FromResult(Build());

        public async Task<EmailRequest> BuildAsync<T>(T model)
        {
            if (_compiler == null || _resolver == null || _provider == null)
                return Request;

            string templateName = _resolver?.Resolve(model);
            string templateContent = await _provider?.GetTemplate(templateName);
            string compiledBody = await _compiler?.CompileBody(model, templateContent);

            Request.Body = compiledBody;
            return Request;
        }
    }
}