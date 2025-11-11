using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Resolvers.ViewModel.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ViewModelTemplateResolverTests
    {
        [TestMethod]
        public void ViewModelTemplateResolver_ByType_ShouldReturnCorrectTemplate()
        {
            ITemplateResolver resolver = new ViewModelTemplateResolver();
            Assert.AreEqual("Test", resolver.Resolve<TestMailModel>());
        }

        [TestMethod]
        public void ViewModelTemplateResolver_ByInstance_ShouldReturnCorrectTemplate()
        {
            TestMailModel model = new();
            ITemplateResolver resolver = new ViewModelTemplateResolver();
            Assert.AreEqual("Test", resolver.Resolve(model));
        }

        [TestMethod]
        public void ViewModelTemplateResolver_ViewModelType_ShouldReturnCorrectTemplate()
        {
            TestViewModel model = new();
            ITemplateResolver resolver = new ViewModelTemplateResolver();
            Assert.AreEqual("Test", resolver.Resolve(model));
        }
    }
}