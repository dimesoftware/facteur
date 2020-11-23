using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.TemplateProviders.IO.Tests
{
    [TestClass]
    public class AppDirectoryTemplateProviderTests
    {
        [TestMethod]
        public async Task AppDirectoryTemplateProvider_()
        {
            ITemplateProvider provider = new AppDirectoryTemplateProvider();
            string template = await provider.GetFile("Templates", "Test.dhtml");

            Assert.IsFalse(string.IsNullOrEmpty(template));
        }
    }
}
