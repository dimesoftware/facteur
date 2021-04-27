using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Resolvers.ViewModel.Tests
{
    [TestClass]
    public class ViewModelTemplateResolverTests
    {
        [TestMethod]
        public void ViewModelTemplateResolver_ByType_ShouldReturnCorrectTemplate()
        {
            ITemplateResolver resolver = new ViewModelTemplateResolver();
            Assert.IsTrue(resolver.Resolve<TestMailModel>() == "Test");
        }

        [TestMethod]
        public void ViewModelTemplateResolver_ByInstance_ShouldReturnCorrectTemplate()
        {
            TestMailModel model = new();
            ITemplateResolver resolver = new ViewModelTemplateResolver();
            Assert.IsTrue(resolver.Resolve(model) == "Test");
        }

        [TestMethod]
        public void ViewModelTemplateResolver_ViewModelType_ShouldReturnCorrectTemplate()
        {
            TestViewModel model = new();
            ITemplateResolver resolver = new ViewModelTemplateResolver();
            Assert.IsTrue(resolver.Resolve(model) == "Test");
        }
    }
}
