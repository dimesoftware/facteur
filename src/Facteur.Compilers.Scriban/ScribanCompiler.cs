using System.IO;
using System.Threading.Tasks;
using Scriban;

namespace Facteur
{
    /// <summary>
    ///
    /// </summary>
    public class ScribanCompiler : ITemplateCompiler
    {
        public ScribanCompiler(ITemplateProvider templateProvider)
        {
            Provider = templateProvider;
        }

        private ITemplateProvider Provider { get; }

        /// <summary>
        /// Gets the body for specified templates & data model
        /// </summary>
        /// <typeparam name="T">The mail model</typeparam>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="model">The model to compile into the e-mail template</param>
        /// <returns>A populated e-mail body</returns>
        public async Task<string> CompileBody<T>(T model, string templateName)
        {
            string templatePath = Path.Combine(templateName + TemplateSettings.Instance.Extension);
            string fileContent = await Provider.GetFile(TemplateSettings.Instance.RelativePath, templatePath).ConfigureAwait(false);
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
            DefaultViewModel model = new DefaultViewModel { Text = text };
            return await CompileBody(model, templateName).ConfigureAwait(false);
        }
    }
}