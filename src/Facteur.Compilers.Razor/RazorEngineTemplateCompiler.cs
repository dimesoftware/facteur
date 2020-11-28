using System.IO;
using System.Threading.Tasks;
using RazorEngine.Templating;

namespace Facteur
{
    /// <summary>
    ///
    /// </summary>
    public class RazorEngineTemplateCompiler : ITemplateCompiler
    {
        public RazorEngineTemplateCompiler()
        {
        }

        /// <summary>
        /// Gets the body for specified templates and data model
        /// </summary>
        /// <typeparam name="T">The mail model</typeparam>
        /// <param name="fileContent">The body template</param>
        /// <param name="model">The model to compile into the e-mail template</param>
        /// <returns>A populated e-mail body</returns>
        public Task<string> CompileBody<T>(T model, string fileContent)
        {
            string compiledBody = RazorEngine.Engine.Razor.RunCompile(fileContent, fileContent, model.GetType(), model);
            return Task.FromResult(compiledBody);
        }

        /// <summary>
        /// Gets the body for non-specified templates & data model
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="text">The text.</param>
        /// <returns>A populated e-mail body</returns>
        public async Task<string> CompileBody(string templateName, string text)
        {
            DefaultViewModel model = new DefaultViewModel { Text = text };
            return await CompileBody(model, templateName).ConfigureAwait(false);
        }
    }
}