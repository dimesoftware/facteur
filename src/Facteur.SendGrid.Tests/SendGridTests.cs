using System;
using System.Diagnostics.CodeAnalysis;
using Facteur.SendGrid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SendGridTests
    {
        [TestMethod]
        public void Sendgrid_SendMail_KeyIsNull_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("majev@getnada.com")
                .Build();

            Assert.ThrowsException<ArgumentNullException>(() => new SendGridMailer(null));
        }

        [TestMethod]
        public void Sendgrid_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("majev@getnada.com")
                .Build();

            Assert.ThrowsException<ArgumentNullException>(() => new SendGridMailer(""));
        }

        [TestMethod]
        public void Sendgrid_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("majev@getnada.com")
                .Build();

            IMailer mailer = new SendGridMailer("MySGKey");
        }
    }
}