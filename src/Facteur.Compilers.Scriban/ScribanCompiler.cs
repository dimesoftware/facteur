using System.Threading.Tasks;
using Scriban;

namespace Facteur
{
    /// <summary>
    ///
    /// </summary>
    public class ScribanCompiler : ITemplateCompiler
    {
        /// <summary>
        /// Gets the body for specified templates & data model
        /// </summary>
        /// <typeparam name="T">The mail model</typeparam>
        /// <param name="fileContent">Name of the template.</param>
        /// <param name="model">The model to compile into the e-mail template</param>
        /// <returns>A populated e-mail body</returns>
        public async Task<string> CompileBody<T>(T model, string fileContent)
        {
            Template template = Template.Parse(fileContent);
            return await template.RenderAsync(model);
        }

        /// <summary>
        /// Gets the body for non-specified templates & data model
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="text">The text.</param>
        /// <returns>A populated e-mail body</returns>
        public async Task<string> CompileBody(string templateName, string text)
        {
            DefaultViewModel model = new() { Text = text };
            return await CompileBody(model, templateName);
        }
    }
}