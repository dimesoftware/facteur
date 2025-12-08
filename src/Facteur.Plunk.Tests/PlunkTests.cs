using System;
using System.Diagnostics.CodeAnalysis;
using Facteur;
using Facteur.Plunk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PlunkTests
    {
        [TestMethod]
        public void Plunk_SendMail_KeyIsNull_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.ThrowsException<ArgumentNullException>(() => new PlunkMailer(null));
        }

        [TestMethod]
        public void Plunk_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.ThrowsException<ArgumentNullException>(() => new PlunkMailer(""));
        }

        [TestMethod]
        public void Plunk_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            IMailer mailer = new PlunkMailer("MyPlunkKey");
            Assert.IsNotNull(mailer);
        }
    }
}

