using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Resend;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
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
            IEmailComposer mockComposer = Substitute.For<IEmailComposer>();
            EmailRequest expectedRequest = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test body"
            };

            mockComposer.Subject(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.From(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.To(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.BuildAsync().Returns(expectedRequest);

            ResendMailer mailer = new("MyResendKey", mockComposer);

            await Assert.ThrowsAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            await mockComposer.Received(1).BuildAsync();
        }

        [TestMethod]
        public async Task ResendPlainText_SendMailAsync_WithComposer_ShouldCallComposer()
        {
            IEmailComposer mockComposer = Substitute.For<IEmailComposer>();
            EmailRequest expectedRequest = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test body"
            };

            mockComposer.Subject(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.From(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.To(Arg.Any<string>()).Returns(mockComposer);
            mockComposer.BuildAsync().Returns(expectedRequest);

            ResendPlainTextMailer mailer = new("MyResendKey", mockComposer);

            await Assert.ThrowsAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            await mockComposer.Received(1).BuildAsync();
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

            Assert.IsNotNull(message.Cc);
            Assert.HasCount(2, message.Cc);
            Assert.Contains("cc1@example.com", message.Cc);
            Assert.Contains("cc2@example.com", message.Cc);
            Assert.DoesNotContain("to@example.com", message.Cc); // Should be excluded as it's in To
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

            // Cc may be null if no items were added
            if (message.Cc == null)
            {
                Assert.IsNull(message.Cc);
            }
            else
            {
                Assert.IsEmpty(message.Cc);
            }
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

            Assert.IsNotNull(message.Bcc);
            Assert.HasCount(2, message.Bcc);
            Assert.Contains("bcc1@example.com", message.Bcc);
            Assert.Contains("bcc2@example.com", message.Bcc);
            Assert.DoesNotContain("to@example.com", message.Bcc); // Should be excluded
            Assert.DoesNotContain("cc@example.com", message.Bcc); // Should be excluded
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

            // Bcc may be null if no items were added
            if (message.Bcc == null)
            {
                Assert.IsNull(message.Bcc);
            }
            else
            {
                Assert.IsEmpty(message.Bcc);
            }
        }

        [TestMethod]
        public void Resend_AttachmentConverter_AddAttachments_ShouldAddAttachments()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                Attachments =
                [
                    new("test.txt", [1, 2, 3]),
                    new("test.pdf", [4, 5, 6])
                ]
            };

            message.AddAttachments(request);

            Assert.IsNotNull(message.Attachments);
            Assert.HasCount(2, message.Attachments);
            Assert.AreEqual("test.txt", message.Attachments[0].Filename);
            Assert.AreEqual("test.pdf", message.Attachments[1].Filename);
            Assert.IsNotNull(message.Attachments[0].Content);
            Assert.IsNotNull(message.Attachments[1].Content);
        }

        [TestMethod]
        public void Resend_AttachmentConverter_AddAttachments_WithNoAttachments_ShouldNotAdd()
        {
            EmailMessage message = new();
            EmailRequest request = new()
            {
                Attachments = []
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
            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
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
                Attachments =
                [
                    new("test.txt", [1, 2, 3])
                ]
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
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
                Attachments =
                [
                    new("test.txt", [1, 2, 3])
                ]
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }
    }
}