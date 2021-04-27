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
        /// <param name="text">Name of the template.</param>
        /// <param name="fileContent">The text.</param>
        /// <returns>A populated e-mail body</returns>
        public async Task<string> CompileBody(string text, string fileContent)
        {
            DefaultViewModel model = new() { Text = text };
            return await CompileBody(model, fileContent).ConfigureAwait(false);
        }
    }
}