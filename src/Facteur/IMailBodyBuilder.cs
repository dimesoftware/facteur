using System.Threading.Tasks;

namespace Facteur
{
    public interface IMailBodyBuilder
    {
        IMailBodyBuilder UseCompiler(ITemplateCompiler compiler);

        IMailBodyBuilder UseProvider(ITemplateProvider provider);

        IMailBodyBuilder UseResolver(ITemplateResolver resolver);

        Task<EmailRequest> BuildAsync<T>(EmailRequest<T> request) where T : class;
    }
}