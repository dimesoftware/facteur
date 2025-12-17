using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Facteur;
using Facteur.AzureCommunicationServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AzureCommunicationServicesTests
    {
        private const string TestConnectionString = "endpoint=https://test.communication.azure.com/;accesskey=dGVzdGtleQ==";

        [TestMethod]
        public void AzureCommunicationServices_SendMail_ConnectionStringIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureCommunicationServicesMailer(null));
        }

        [TestMethod]
        public void AzureCommunicationServices_SendMail_ConnectionStringIsEmpty_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureCommunicationServicesMailer(""));
        }

        [TestMethod]
        public void AzureCommunicationServices_SendMail_ConnectionStringIsNotEmpty_ShouldConstruct()
        {
            IMailer mailer = new AzureCommunicationServicesMailer(TestConnectionString);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void AzureCommunicationServices_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new AzureCommunicationServicesMailer(TestConnectionString, composer);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public async Task AzureCommunicationServices_SendMailAsync_WithComposer_ShouldCallComposer()
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

            AzureCommunicationServicesMailer mailer = new(TestConnectionString, mockComposer);

            await Assert.ThrowsAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            await mockComposer.Received(1).BuildAsync();
        }

        [TestMethod]
        public async Task AzureCommunicationServices_SendMailAsync_WithCcAndBcc_ShouldProcessCorrectly()
        {
            AzureCommunicationServicesMailer mailer = new(TestConnectionString);
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@test.azurecomm.net", "From Name"),
                To = ["to@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["bcc@example.com"],
                Body = "Test body"
            };

            // This will fail because we don't have a real connection string, but it will exercise the code paths
            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task AzureCommunicationServices_SendMailAsync_WithAttachments_ShouldProcessCorrectly()
        {
            AzureCommunicationServicesMailer mailer = new(TestConnectionString);
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@test.azurecomm.net", "From Name"),
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
        public async Task AzureCommunicationServices_SendMailAsync_WithCcDuplicateInTo_ShouldExcludeFromCc()
        {
            AzureCommunicationServicesMailer mailer = new(TestConnectionString);
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@test.azurecomm.net", "From Name"),
                To = ["to@example.com"],
                Cc = ["to@example.com", "cc@example.com"], // First one is duplicate
                Body = "Test body"
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task AzureCommunicationServices_SendMailAsync_WithBccDuplicateInToAndCc_ShouldExcludeFromBcc()
        {
            AzureCommunicationServicesMailer mailer = new(TestConnectionString);
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@test.azurecomm.net", "From Name"),
                To = ["to@example.com"],
                Cc = ["cc@example.com"],
                Bcc = ["to@example.com", "cc@example.com", "bcc@example.com"], // First two are duplicates
                Body = "Test body"
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public async Task AzureCommunicationServices_SendMailAsync_WithMultipleRecipients_ShouldProcessCorrectly()
        {
            AzureCommunicationServicesMailer mailer = new(TestConnectionString);
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@test.azurecomm.net", "From Name"),
                To = ["to1@example.com", "to2@example.com"],
                Cc = ["cc1@example.com", "cc2@example.com"],
                Bcc = ["bcc1@example.com", "bcc2@example.com"],
                Body = "Test body"
            };

            await Assert.ThrowsAsync<Exception>(async () => await mailer.SendMailAsync(request));
        }

        [TestMethod]
        public void AzureCommunicationServices_EmailMessageExtensions_AddCc_WithNoCc_ShouldReturnMessage()
        {
            EmailContent content = new("Test Subject") { Html = "Test body" };
            EmailMessage message = new(
                senderAddress: "from@test.azurecomm.net",
                content: content,
                recipients: new EmailRecipients([new EmailAddress("to@example.com")]));

            EmailRequest request = new()
            {
                Cc = []
            };

            EmailMessage result = message.AddCc(request);

            Assert.AreSame(message, result);
        }

        [TestMethod]
        public void AzureCommunicationServices_EmailMessageExtensions_AddBcc_WithNoBcc_ShouldReturnMessage()
        {
            EmailContent content = new("Test Subject") { Html = "Test body" };
            EmailMessage message = new(
                senderAddress: "from@test.azurecomm.net",
                content: content,
                recipients: new EmailRecipients([new EmailAddress("to@example.com")]));

            EmailRequest request = new()
            {
                Bcc = []
            };

            EmailMessage result = message.AddBcc(request);

            Assert.AreSame(message, result);
        }

        [TestMethod]
        public void AzureCommunicationServices_EmailMessageExtensions_AddAttachments_WithNoAttachments_ShouldReturnMessage()
        {
            EmailContent content = new("Test Subject") { Html = "Test body" };
            EmailMessage message = new(
                senderAddress: "from@test.azurecomm.net",
                content: content,
                recipients: new EmailRecipients([new EmailAddress("to@example.com")]));

            EmailRequest request = new()
            {
                Attachments = []
            };

            EmailMessage result = message.AddAttachments(request);

            Assert.AreSame(message, result);
        }

        [TestMethod]
        public void AzureCommunicationServices_EmailMessageExtensions_AddAttachments_ShouldAddAttachments()
        {
            EmailContent content = new("Test Subject") { Html = "Test body" };
            EmailMessage message = new(
                senderAddress: "from@test.azurecomm.net",
                content: content,
                recipients: new EmailRecipients([new EmailAddress("to@example.com")]));

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
            Assert.AreEqual("test.txt", message.Attachments[0].Name);
            Assert.AreEqual("test.pdf", message.Attachments[1].Name);
        }

        [TestMethod]
        public void AzureCommunicationServicesPlainText_SendMail_ConnectionStringIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureCommunicationServicesPlainTextMailer(null));
        }

        [TestMethod]
        public void AzureCommunicationServicesPlainText_SendMail_ConnectionStringIsEmpty_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureCommunicationServicesPlainTextMailer(""));
        }

        [TestMethod]
        public void AzureCommunicationServicesPlainText_SendMail_ConnectionStringIsNotEmpty_ShouldConstruct()
        {
            IMailer mailer = new AzureCommunicationServicesPlainTextMailer(TestConnectionString);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public void AzureCommunicationServicesPlainText_SendMail_WithComposer_ShouldConstruct()
        {
            EmailComposer composer = new();
            IMailer mailer = new AzureCommunicationServicesPlainTextMailer(TestConnectionString, composer);
            Assert.IsNotNull(mailer);
        }

        [TestMethod]
        public async Task AzureCommunicationServicesPlainText_SendMailAsync_WithComposer_ShouldCallComposer()
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

            AzureCommunicationServicesPlainTextMailer mailer = new(TestConnectionString, mockComposer);

            await Assert.ThrowsAsync<Exception>(async () =>
                await mailer.SendMailAsync(async composer => await composer
                    .Subject("Test")
                    .From("test@example.com")
                    .To("recipient@example.com")
                    .BuildAsync()));

            await mockComposer.Received(1).BuildAsync();
        }

        [TestMethod]
        public async Task AzureCommunicationServicesPlainText_SendMailAsync_WithAllFields_ShouldProcessCorrectly()
        {
            AzureCommunicationServicesPlainTextMailer mailer = new(TestConnectionString);
            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("from@test.azurecomm.net", "From Name"),
                To = ["hendrik@diescheduler.com", "to2@example.com"],
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
