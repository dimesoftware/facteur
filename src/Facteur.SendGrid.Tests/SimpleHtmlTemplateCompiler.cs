using System.Text;
using System.Threading.Tasks;

namespace Facteur.Tests
{
    public class SimpleHtmlTemplateCompiler : ITemplateCompiler
    {
        public Task<string> CompileBody<T>(T model, string templateName)
            => Task.FromResult(model.ToString());

        public Task<string> CompileBody(string text, string templateName)
            => Task.FromResult(BuildPage(text));

        private static string BuildPage(string text)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("<html>");
            stringBuilder.Append("<body>");
            stringBuilder.Append("Hello");
            stringBuilder.Append(text);
            stringBuilder.Append("</body>");
            stringBuilder.Append("</html>");
            return stringBuilder.ToString();
        }
    }
}