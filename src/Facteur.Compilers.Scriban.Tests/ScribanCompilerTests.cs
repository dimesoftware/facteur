using System.Threading.Tasks;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Compilers.Scriban.Tests
{
    [TestClass]
    public class ScribanCompilerTests
    {
        [TestMethod]
        public async Task ScribanCompiler_ShouldPopulateTemplate()
        {
            ITemplateCompiler scribanCompiler = new ScribanCompiler();
            string body = await scribanCompiler.CompileBody(new TestMailModel { Name = "Handsome B. Wonderful" }, "Hello {{name}}!");

            Assert.IsTrue(body.Contains("Handsome B. Wonderful"));
        }
    }
}
