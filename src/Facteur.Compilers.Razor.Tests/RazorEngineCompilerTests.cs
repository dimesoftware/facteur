using System.Threading.Tasks;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Compilers.Scriban.Tests
{
    [TestClass]
    public class RazorEngineCompilerTests
    {
        [TestMethod]
        public async Task RazorEngineCompiler_ShouldPopulateTemplate()
        {
            string cshtml = @"<html><body><h1>Hi @Raw(Model.Name)</h1></body></html>";
            ITemplateCompiler compiler = new RazorEngineTemplateCompiler();
            string body = await compiler.CompileBody(new TestMailModel { Name = "Handsome B. Wonderful" }, cshtml);

            Assert.IsTrue(body.Contains("Handsome B. Wonderful"));
        }
    }
}
