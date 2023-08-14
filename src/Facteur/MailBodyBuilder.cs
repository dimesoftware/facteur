using System;
using System.Threading.Tasks;

namespace Facteur
{
    public class MailBodyBuilder : IMailBodyBuilder
    {
        private ITemplateCompiler _compiler;
        private ITemplateProvider _provider;
        private ITemplateResolver _resolver;

        public MailBodyBuilder()
        {
        }

        public MailBodyBuilder(ITemplateCompiler compiler, ITemplateProvider provider, ITemplateResolver resolver)
            : this()
        {
            _compiler = compiler;
            _provider = provider;
            _resolver = resolver;
        }

        public IMailBodyBuilder UseCompiler(ITemplateCompiler compiler) => Use(() => _compiler = compiler);

        public IMailBodyBuilder UseProvider(ITemplateProvider provider) => Use(() => _provider = provider);

        public IMailBodyBuilder UseResolver(ITemplateResolver resolver) => Use(() => _resolver = resolver);

        public async Task<EmailRequest> BuildAsync<T>(EmailRequest<T> request) where T : class
        {
            string templateName = _resolver.Resolve(request.Model);
            string templateContent = await _provider.GetTemplate(templateName);
            string compiledBody = await _compiler.CompileBody(request.Model, templateContent);

            return request.Copy(compiledBody);
        }

        private MailBodyBuilder Use<T>(Func<T> func)
        {
            func();
            return this;
        }
    }
}