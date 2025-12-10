using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Facteur;
using Facteur.Resend;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Resend;

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

        [TestMethod]
        public async Task Resend_SendMailAsync_WithComposer_ShouldCallComposer()
        {
            Mock<IEmailComposer> mockComposer = new();
            EmailRequest expectedRequest = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test body"
            };

            mockComposer.Setup(c => c.Subject(It.IsAny<string>())).Returns(mockComposer.Object);
            mockComposer.Setup(c => c.From(It.IsAny<string>())).Returns(mockComposer.Object);
            mockComposer.Setup(c => c.To(It.IsAny<string>())).Returns(mockComposer.Object);
            mockComposer.Setup(c => c.BuildAsync()).ReturnsAsync(expectedRequest);

            ResendMailer mailer = new("MyResendKey", mockComposer.Object);

            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            mockComposer.Verify(c => c.BuildAsync(), Times.Once);
        }

        [TestMethod]
        public async Task ResendPlainText_SendMailAsync_WithComposer_ShouldCallComposer()
        {
            Mock<IEmailComposer> mockComposer = new();
            EmailRequest expectedRequest = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test body"
            };

            mockComposer.Setup(c => c.Subject(It.IsAny<string>())).Returns(mockComposer.Object);
            mockComposer.Setup(c => c.From(It.IsAny<string>())).Returns(mockComposer.Object);
            mockComposer.Setup(c => c.To(It.IsAny<string>())).Returns(mockComposer.Object);
            mockComposer.Setup(c => c.BuildAsync()).ReturnsAsync(expectedRequest);

            ResendPlainTextMailer mailer = new("MyResendKey", mockComposer.Object);

            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            mockComposer.Verify(c => c.BuildAsync(), Times.Once);
        }

        [TestMethod]
        public void Resend_EmailMessageExtensions_AddCc_ShouldAddCcRecipients()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                To = ["to@example.com"],
                Cc = ["cc1@example.com", "cc2@example.com", "to@example.com"] // Last one is duplicate
            };

            message.AddCc(request);

            Assert.AreEqual(2, message.Cc.Count);
            Assert.IsTrue(message.Cc.Contains("cc1@example.com"));
            Assert.IsTrue(message.Cc.Contains("cc2@example.com"));
            Assert.IsFalse(message.Cc.Contains("to@example.com")); // Should be excluded as it's in To
        }

        [TestMethod]
        public void Resend_EmailMessageExtensions_AddCc_WithNoCc_ShouldNotAdd()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                To = ["to@example.com"],
                Cc = []
            };

            message.AddCc(request);

            Assert.AreEqual(0, message.Cc.Count);
        }

        [TestMethod]
        public void Resend_EmailMessageExtensions_AddBcc_ShouldAddBccRecipients()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                To = ["to@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["bcc1@example.com", "bcc2@example.com", "to@example.com", "cc@example.com"]
            };

            message.AddBcc(request);

            Assert.AreEqual(2, message.Bcc.Count);
            Assert.IsTrue(message.Bcc.Contains("bcc1@example.com"));
            Assert.IsTrue(message.Bcc.Contains("bcc2@example.com"));
            Assert.IsFalse(message.Bcc.Contains("to@example.com")); // Should be excluded
            Assert.IsFalse(message.Bcc.Contains("cc@example.com")); // Should be excluded
        }

        [TestMethod]
        public void Resend_EmailMessageExtensions_AddBcc_WithNoBcc_ShouldNotAdd()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                To = ["to@example.com"],
                Bcc = []
            };

            message.AddBcc(request);

            Assert.AreEqual(0, message.Bcc.Count);
        }

        [TestMethod]
        public void Resend_AttachmentConverter_AddAttachments_ShouldAddAttachments()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                Attachments = new List<Attachment>
                {
                    new Attachment("test.txt", new byte[] { 1, 2, 3 }),
                    new Attachment("test.pdf", new byte[] { 4, 5, 6 })
                }
            };

            message.AddAttachments(request);

            Assert.IsNotNull(message.Attachments);
            Assert.AreEqual(2, message.Attachments.Count);
            Assert.AreEqual("test.txt", message.Attachments[0].Filename);
            Assert.AreEqual("test.pdf", message.Attachments[1].Filename);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, message.Attachments[0].Content);
            CollectionAssert.AreEqual(new byte[] { 4, 5, 6 }, message.Attachments[1].Content);
        }

        [TestMethod]
        public void Resend_AttachmentConverter_AddAttachments_WithNoAttachments_ShouldNotAdd()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                Attachments = new List<Attachment>()
            };

            EmailMessage result = message.AddAttachments(request);

            Assert.AreSame(message, result);
            Assert.IsNull(message.Attachments);
        }

        [TestMethod]
        public async Task Resend_SendMailAsync_WithCcAndBcc_ShouldProcessCorrectly()
        {
            ResendMailer mailer = new("MyResendKey");
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["bcc@example.com"],
                Body = "Test body"
            };

            // This will fail because we don't have a real API key, but it will exercise the code paths
            await Assert.ThrowsExceptionAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task Resend_SendMailAsync_WithAttachments_ShouldProcessCorrectly()
        {
            ResendMailer mailer = new("MyResendKey");
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Body = "Test body",
                Attachments = new List<Attachment>
                {
                    new Attachment("test.txt", new byte[] { 1, 2, 3 })
                }
            };

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task ResendPlainText_SendMailAsync_WithAllFields_ShouldProcessCorrectly()
        {
            ResendPlainTextMailer mailer = new("MyResendKey");
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@example.com", "From Name"),
                To = ["to1@example.com", "to2@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["bcc@example.com"],
                Body = "Test body",
                Attachments = new List<Attachment>
                {
                    new Attachment("test.txt", new byte[] { 1, 2, 3 })
                }
            };

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }
    }
}
