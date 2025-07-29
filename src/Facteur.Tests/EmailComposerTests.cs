using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

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

        [TestMethod]
        public void EmailComposer_Reset_ShouldCreateNewEmailRequest()
        {
            // Arrange
            EmailComposer composer = new();

            // Act - Set some properties
            composer.Subject("Test Subject")
                   .From("test@example.com")
                   .To("recipient@example.com")
                   .Body("Test Body");

            // Verify initial state
            EmailRequest firstRequest = composer.Build();
            Assert.AreEqual("Test Subject", firstRequest.Subject);
            Assert.AreEqual("test@example.com", firstRequest.From.Email);
            CollectionAssert.AreEqual(new List<string> { "recipient@example.com" }, firstRequest.To.ToArray());
            Assert.AreEqual("Test Body", firstRequest.Body);
        }

        [TestMethod]
        public void EmailComposer_Reset_ShouldBeFluent()
        {
            // Arrange
            EmailComposer composer = new();

            // Act - Use Reset() fluently in a chain
            EmailRequest request = composer.Reset()
                .Subject("New Subject")
                .From("new@example.com")
                .To("new@recipient.com")
                .Body("New Body")
                .Build();

            // Assert - Should work fluently and create clean request
            Assert.AreEqual("New Subject", request.Subject);
            Assert.AreEqual("new@example.com", request.From.Email);
            CollectionAssert.AreEqual(new List<string> { "new@recipient.com" }, request.To.ToArray());
            Assert.AreEqual("New Body", request.Body);
        }
    }
}