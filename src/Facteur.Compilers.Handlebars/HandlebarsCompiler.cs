using System.Threading.Tasks;
using HandlebarsDotNet;

namespace Facteur
{
    /// <summary>
    /// Represents a template compiler that uses the Handlebars template engine.
    /// </summary>
    public class HandlebarsCompiler : ITemplateCompiler
    {
        /// <summary>
        /// Gets the body for specified templates & data model
        /// </summary>
        /// <typeparam name="T">The mail model</typeparam>
        /// <param name="fileContent">Name of the template.</param>
        /// <param name="model">The model to compile into the email template</param>
        /// <returns>A populated email body</returns>
        public Task<string> CompileBody<T>(T model, string fileContent)
        {
            HandlebarsTemplate<object, object> template = Handlebars.Compile(fileContent);
            string result = template(model);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Gets the body for non-specified templates & data model
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="text">The text.</param>
        /// <returns>A populated email body</returns>
        public Task<string> CompileBody(string templateName, string text)
        {
            DefaultViewModel model = new() { Text = text };
            return CompileBody(model, templateName);
        }
    }
}
