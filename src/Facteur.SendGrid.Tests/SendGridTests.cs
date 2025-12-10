using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.Throws<ArgumentNullException>(() => new SendGridMailer(null));
        }

        [TestMethod]
        public void Sendgrid_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            Assert.Throws<ArgumentNullException>(() => new SendGridMailer(""));
        }

        [TestMethod]
        public async Task Sendgrid_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            EmailComposer composer = new();
            EmailRequest request = composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("majev@getnada.com")
                .Build();

            IMailer mailer = new SendGridMailer("MySGKey");
            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }
    }
}