using System;
using Facteur.SendGrid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    public class SendGridTests
    {
        [TestMethod]
        public void Sendgrid_SendMail_KeyIsNull_ShouldThrowException()
        {
            EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("majev@getnada.com")
                .Build();

            Assert.ThrowsException<ArgumentNullException>(() => new SendGridMailer(null));
        }

        [TestMethod]
        public void Sendgrid_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("majev@getnada.com")
                .Build();

            Assert.ThrowsException<ArgumentNullException>(() => new SendGridMailer(""));
        }

        [TestMethod]
        public void Sendgrid_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("majev@getnada.com")
                .Build();

            IMailer mailer = new SendGridMailer("MySGKey");
        }
    }
}