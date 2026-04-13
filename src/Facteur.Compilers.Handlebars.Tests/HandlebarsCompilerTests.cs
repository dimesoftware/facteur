using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Compilers.Handlebars.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class HandlebarsCompilerTests
    {
        [TestMethod]
        public async Task HandlebarsCompiler_UseModel_ShouldPopulateTemplate()
        {
            ITemplateCompiler handlebarsCompiler = new HandlebarsCompiler();
            string body = await handlebarsCompiler.CompileBody(new TestMailModel { Name = "Handsome B. Wonderful" }, "Hello {{Name}}!");

            Assert.Contains("Handsome B. Wonderful", body);
        }

        [TestMethod]
        public async Task HandlebarsCompiler_UseString_ShouldPopulateTemplate()
        {
            string template = @"<html><body><h1>Hello {{Text}}!</h1></body></html>";
            ITemplateCompiler compiler = new HandlebarsCompiler();
            string body = await compiler.CompileBody("Handsome B. Wonderful", template);

            Assert.Contains("Handsome B. Wonderful", body);
        }
    }
}
