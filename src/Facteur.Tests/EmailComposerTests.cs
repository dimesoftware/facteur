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
                () => composer.From("info@facteur.com").Build());
        }

        [TestMethod]
        public void EmailComposer_Build_HasEmptyTo_ShouldThrowException()
        {
            EmailComposer composer = new();
            Assert.ThrowsException<ArgumentNullException>(
                () => composer.From("info@facteur.com").Subject("Hello world").Body("Test").Build());
        }
    }
}