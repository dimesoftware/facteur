using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Compilers.Fluid.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FluidCompilerTests
    {
        [TestMethod]
        public async Task FluidCompiler_UseModel_ShouldPopulateTemplate()
        {
            ITemplateCompiler fluidCompiler = new FluidCompiler();
            string body = await fluidCompiler.CompileBody(new TestMailModel { Name = "Handsome B. Wonderful" }, "Hello {{ Name }}!");

            Assert.Contains("Handsome B. Wonderful", body);
        }

        [TestMethod]
        public async Task FluidCompiler_UseString_ShouldPopulateTemplate()
        {
            string template = @"<html><body><h1>Hello {{ text }}!</h1></body></html>";
            ITemplateCompiler compiler = new FluidCompiler();
            string body = await compiler.CompileBody("Handsome B. Wonderful", template);

            Assert.Contains("Handsome B. Wonderful", body);
        }
    }
}
