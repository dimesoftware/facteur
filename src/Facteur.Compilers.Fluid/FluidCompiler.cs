using System.Threading.Tasks;
using Fluid;

namespace Facteur
{
    /// <summary>
    /// Represents a template compiler that uses the Fluid (Liquid) template engine.
    /// </summary>
    public class FluidCompiler : ITemplateCompiler
    {
        private static readonly FluidParser Parser = new();

        /// <summary>
        /// Gets the body for specified templates & data model
        /// </summary>
        /// <typeparam name="T">The mail model</typeparam>
        /// <param name="fileContent">Name of the template.</param>
        /// <param name="model">The model to compile into the email template</param>
        /// <returns>A populated email body</returns>
        public async Task<string> CompileBody<T>(T model, string fileContent)
        {
            IFluidTemplate template = Parser.Parse(fileContent);
            TemplateOptions options = new();
            options.MemberAccessStrategy.Register(model.GetType());
            TemplateContext context = new(model, options);
            return await template.RenderAsync(context);
        }

        /// <summary>
        /// Gets the body for non-specified templates & data model
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="text">The text.</param>
        /// <returns>A populated email body</returns>
        public async Task<string> CompileBody(string templateName, string text)
        {
            DefaultViewModel model = new() { Text = text };
            return await CompileBody(model, templateName);
        }
    }
}
