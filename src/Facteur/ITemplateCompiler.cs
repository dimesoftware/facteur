using System.Threading.Tasks;

namespace Facteur
{
    public interface ITemplateCompiler
    {
        Task<string> CompileBody<T>(T model, string templateContent);

        Task<string> CompileBody(string text, string templateContent);
    }
}