using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// Represents a type that can retrieve email templates.
    /// </summary>
    public interface ITemplateProvider
    {
        /// <summary>
        /// Gets the email template
        /// </summary>
        /// <param name="name">The template's name</param>
        /// <returns>The template contents</returns>
        Task<string> GetTemplate(string name);
    }
}