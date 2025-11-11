using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.TemplateProviders.IO.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AppDirectoryTemplateProviderTests
    {
        [TestMethod]
        public async Task AppDirectoryTemplateProvider_HasCorrectData_ShouldReturnPath()
        {
            ITemplateProvider provider = new AppDirectoryTemplateProvider("Templates", ".sbnhtml");
            string template = await provider.GetTemplate("Test");

            Assert.IsFalse(string.IsNullOrEmpty(template));
        }

        [TestMethod]
        public void AppDirectoryTemplateProvider_HasIncorrectData_ShouldRaiseException()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new FaultyTemplateProvider());
        }
    }
}