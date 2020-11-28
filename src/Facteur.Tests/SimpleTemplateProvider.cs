using System.Text;
using System.Threading.Tasks;

namespace Facteur.Tests
{
    public class SimpleTemplateProvider : ITemplateProvider
    {
        public Task<string> GetFile(string fileName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<html>");
            stringBuilder.Append("<body>");
            stringBuilder.Append("{{_txt_}}");
            stringBuilder.Append("</body>");
            stringBuilder.Append("</html>");
            return Task.FromResult(stringBuilder.ToString());
        }
    }
}