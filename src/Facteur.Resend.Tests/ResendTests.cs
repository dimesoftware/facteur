using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Resend;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResendTests
    {
        [TestMethod]
        public void Resend_SendMail_KeyIsNull_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.Throws<ArgumentNullException>(() => new ResendMailer(null));
        }

        [TestMethod]
        public void Resend_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.Throws<ArgumentNullException>(() => new ResendMailer(""));
        }

        [TestMethod]
        public void Resend_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            IMailer mailer = new ResendMailer("MyResendKey");
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void Resend_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new ResendMailer("MyResendKey", composer);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void ResendPlainText_SendMail_KeyIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResendPlainTextMailer(null));
        }

        [TestMethod]
        public void ResendPlainText_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResendPlainTextMailer(""));
        }

        [TestMethod]
        public void ResendPlainText_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            IMailer mailer = new ResendPlainTextMailer("MyResendKey");
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void ResendPlainText_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new ResendPlainTextMailer("MyResendKey", composer);
            Assert.IsNotNull(mailer);
        }
    }
}
