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
            ITemplateCompiler compiler = new RazorEngineTemplateCompiler(new AppDirectoryTemplateProvider());
            string body = await compiler.CompileBody(new TestMailModel { Name = "Handsome B. Wonderful" }, "Test");

            Assert.IsTrue(body.Contains("Handsome B. Wonderful"));
        }
    }
}
