using System.Text;
using System.Threading.Tasks;

namespace Facteur.Tests
{
    public class SimpleHtmlTemplateCompiler : ITemplateCompiler
    {
        public Task<string> CompileBody<T>(T model, string templateName)
            => Task.FromResult(templateName.Replace("_txt_", model.ToString()));

        public Task<string> CompileBody(string text, string templateName)
            => Task.FromResult(text);
    }
}