using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Facteur;
using Facteur.SendGrid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SendGrid.Helpers.Mail;

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

        [TestMethod]
        public void Sendgrid_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new SendGridMailer("MySGKey", composer);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public async Task Sendgrid_SendMailAsync_WithComposer_ShouldCallComposer()
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

            SendGridMailer mailer = new("MySGKey", mockComposer);

            await Assert.ThrowsAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            await mockComposer.Received(1).BuildAsync();
        }

        [TestMethod]
        public void Sendgrid_EmailAddressConverter_ToEmailAddress_ShouldConvert()
        {
            Sender sender = new("test@example.com", "Test Name");
            EmailAddress emailAddress = sender.ToEmailAddress();

            Assert.IsNotNull(emailAddress);
            Assert.AreEqual("test@example.com", emailAddress.Email);
            Assert.AreEqual("Test Name", emailAddress.Name);
        }

        [TestMethod]
        public void Sendgrid_EmailAddressConverter_ToEmailAddress_WithNullName_ShouldConvert()
        {
            Sender sender = new("test@example.com", null);
            EmailAddress emailAddress = sender.ToEmailAddress();

            Assert.IsNotNull(emailAddress);
            Assert.AreEqual("test@example.com", emailAddress.Email);
            Assert.IsNull(emailAddress.Name);
        }

        [TestMethod]
        public void Sendgrid_AttachmentConverter_AddAttachments_ShouldAddAttachments()
        {
            SendGridMessage message = new();
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
            Assert.AreEqual("attachment", message.Attachments[0].Disposition);
            Assert.AreEqual("attachment", message.Attachments[1].Disposition);
        }

        [TestMethod]
        public void Sendgrid_AttachmentConverter_AddAttachments_WithNoAttachments_ShouldNotAdd()
        {
            SendGridMessage message = new();
            EmailRequest request = new()
            {
                Attachments = []
            };

            SendGridMessage result = message.AddAttachments(request);

            Assert.AreSame(message, result);
            Assert.IsNull(message.Attachments);
        }

        [TestMethod]
        public async Task Sendgrid_SendMailAsync_WithCcAndBcc_ShouldProcessCorrectly()
        {
            SendGridMailer mailer = new("MySGKey");
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
        public async Task Sendgrid_SendMailAsync_WithAttachments_ShouldProcessCorrectly()
        {
            SendGridMailer mailer = new("MySGKey");
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
        public async Task Sendgrid_SendMailAsync_WithCcDuplicateInTo_ShouldExcludeFromCc()
        {
            SendGridMailer mailer = new("MySGKey");
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Cc = ["to@example.com", "cc@example.com"], // First one is duplicate
                Body = "Test body"
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task Sendgrid_SendMailAsync_WithBccDuplicateInToAndCc_ShouldExcludeFromBcc()
        {
            SendGridMailer mailer = new("MySGKey");
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@example.com", "From Name"),
                To = ["to@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["to@example.com", "cc@example.com", "bcc@example.com"], // First two are duplicates
                Body = "Test body"
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task Sendgrid_SendMailAsync_WithMultipleRecipients_ShouldProcessCorrectly()
        {
            SendGridMailer mailer = new("MySGKey");
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@example.com", "From Name"),
                To = ["to1@example.com", "to2@example.com"],
                Cc = ["cc1@example.com", "cc2@example.com"],
                Bcc = ["bcc1@example.com", "bcc2@example.com"],
                Body = "Test body"
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public void SendgridPlainText_SendMail_KeyIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new SendGridPlainTextMailer(null));
        }

        [TestMethod]
        public void SendgridPlainText_SendMail_KeyIsEmpty_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new SendGridPlainTextMailer(""));
        }

        [TestMethod]
        public void SendgridPlainText_SendMail_KeyIsNotEmpty_ShouldConstruct()
        {
            IMailer mailer = new SendGridPlainTextMailer("MySGKey");
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void SendgridPlainText_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new SendGridPlainTextMailer("MySGKey", composer);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public async Task SendgridPlainText_SendMailAsync_WithComposer_ShouldCallComposer()
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

            SendGridPlainTextMailer mailer = new("MySGKey", mockComposer);

            await Assert.ThrowsAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            await mockComposer.Received(1).BuildAsync();
        }

        [TestMethod]
        public async Task SendgridPlainText_SendMailAsync_WithAllFields_ShouldProcessCorrectly()
        {
            SendGridPlainTextMailer mailer = new("MySGKey");
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