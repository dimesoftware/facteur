using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// Represents a type that uses a templating engine to populate email template with data from the POCOs.
    /// </summary>
    public interface ITemplateCompiler
    {
        Task<string> CompileBody<T>(T model, string templateContent);

        Task<string> CompileBody(string text, string templateContent);
    }
}