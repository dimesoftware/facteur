using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Compilers.Scriban.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ScribanCompilerTests
    {
        [TestMethod]
        public async Task ScribanCompiler_UseModel_ShouldPopulateTemplate()
        {
            ITemplateCompiler scribanCompiler = new ScribanCompiler();
            string body = await scribanCompiler.CompileBody(new TestMailModel { Name = "Handsome B. Wonderful" }, "Hello {{name}}!");

            Assert.Contains("Handsome B. Wonderful", body);
        }

        [TestMethod]
        public async Task ScribanCompiler_UseString_ShouldPopulateTemplate()
        {
            string cshtml = @"<html><body><h1>Hello {{text}}!</h1></body></html>";
            ITemplateCompiler compiler = new ScribanCompiler();
            string body = await compiler.CompileBody("Handsome B. Wonderful", cshtml);

            Assert.Contains("Handsome B. Wonderful", body);
        }
    }
}