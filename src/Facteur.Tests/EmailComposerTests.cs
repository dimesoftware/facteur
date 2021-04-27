using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    public class EmailComposerTests
    {
        [TestMethod]
        public void EmailComposer_Build_HasEmptyFrom_ShouldThrowException()
        {
            EmailComposer composer = new();
            Assert.ThrowsException<ArgumentNullException>(
                () => composer.Build());
        }

        [TestMethod]
        public void EmailComposer_Build_HasEmptySubject_ShouldThrowException()
        {
            EmailComposer composer = new();
            Assert.ThrowsException<ArgumentNullException>(
                () => composer.SetFrom("info@facteur.com").Build());
        }

        [TestMethod]
        public void EmailComposer_Build_HasEmptyTo_ShouldThrowException()
        {
            EmailComposer composer = new();
            Assert.ThrowsException<ArgumentNullException>(
                () => composer.SetFrom("info@facteur.com").SetSubject("Hello world").SetBody("Test").Build());
        }
    }
}