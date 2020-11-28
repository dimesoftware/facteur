using System;
using System.Threading.Tasks;

namespace Facteur
{
    public class MailFactory : IMailFactory
    {
        private ITemplateCompiler _compiler;
        private ITemplateProvider _provider;
        private ITemplateResolver _resolver;
        private IMailer _mailer;

        public IMailFactory UseCompiler(ITemplateCompiler compiler) => Use(() => _compiler = compiler);

        public IMailFactory UseProvider(ITemplateProvider provider) => Use(() => _provider = provider);

        public IMailFactory UseResolver(ITemplateResolver resolver) => Use(() => _resolver = resolver);

        public IMailFactory UseMailer(IMailer mailer) => Use(() => _mailer = mailer);

        private MailFactory Use<T>(Func<T> func)
        {
            func();
            return this;
        }

        public async Task SendMailAsync(EmailRequest request)
            => await _mailer.SendMailAsync(request).ConfigureAwait(false);

        public async Task SendMailAsync<T>(EmailRequest<T> request) where T : class
        {
            string templateName = _resolver.Resolve(request.Model);
            string templateContent = await _provider.GetFile(templateName).ConfigureAwait(false);

            request.Body = await _compiler.CompileBody(request.Model, templateContent).ConfigureAwait(false);
            await _mailer.SendMailAsync(request).ConfigureAwait(false);
        }
    }
}