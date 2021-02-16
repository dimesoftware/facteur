using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.TemplateProviders.IO.Tests
{
    [TestClass]
    public class AppDirectoryTemplateProviderTests
    {
        [TestMethod]
        public async Task AppDirectoryTemplateProvider_HasCorrectData_ShouldReturnPath()
        {
            ITemplateProvider provider = new AppDirectoryTemplateProvider("Templates", ".dhtml");
            string template = await provider.GetTemplate("Test");

            Assert.IsFalse(string.IsNullOrEmpty(template));
        }

        [TestMethod]
        public void AppDirectoryTemplateProvider_HasIncorrectData_ShouldRaiseException()
        {
            Assert.ThrowsException<DirectoryNotFoundException>(() => new FaultyTemplateProvider());
        }
    }
}
