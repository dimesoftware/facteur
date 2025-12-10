using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur;
using Facteur.MsGraph;
using Microsoft.Graph.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MsGraphTests
    {
        [TestMethod]
        [DataRow("", "tenantId", "clientSecret", "from")]
        [DataRow("clientId", "", "clientSecret", "from")]
        [DataRow("clientId", "tenantId", "", "from")]
        [DataRow("clientId", "tenantId", "clientSecret", "")]
        public void Graph_SendMail_InvalidParameter_ShouldThrowArgumentNullException(string clientId, string tenantId, string clientSecret, string from)
            => Assert.Throws<ArgumentNullException>(() => new GraphCredentials(clientId, tenantId, clientSecret, @from));

        [TestMethod]
        public async Task Graph_SendMail_ShouldSend()
        {
            GraphCredentials credentials = new("clientId", "tenantId", "secret", "from");

            EmailComposer composer = new();
            EmailRequest request = await composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("tibipi@getnada.com")
                .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });

            Mock<IMailer> mock = new();
            mock.Setup(foo => foo.SendMailAsync(request)).Returns(Task.CompletedTask);
        }
    }

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MsGraphAttachmentConverterTests
    {
        [TestMethod]
        public void MsGraph_AttachmentConverter_AddAttachments_ShouldConvertAttachments()
        {
            EmailRequest request = new()
            {
                Attachments =
                [
                    new("test.txt", [1, 2, 3]),
                    new("test.pdf", [4, 5, 6])
                ]
            };

            List<Microsoft.Graph.Models.Attachment> attachments = request.AddAttachments();

            Assert.IsNotNull(attachments);
            Assert.HasCount(2, attachments);
            Assert.IsInstanceOfType(attachments[0], typeof(FileAttachment));
            Assert.IsInstanceOfType(attachments[1], typeof(FileAttachment));
            
            FileAttachment firstAttachment = (FileAttachment)attachments[0];
            Assert.AreEqual("test.txt", firstAttachment.Name);
            byte[] expectedBytes1 = [1, 2, 3];
            CollectionAssert.AreEqual(expectedBytes1, firstAttachment.ContentBytes);

            FileAttachment secondAttachment = (FileAttachment)attachments[1];
            Assert.AreEqual("test.pdf", secondAttachment.Name);
            byte[] expectedBytes2 = [4, 5, 6];
            CollectionAssert.AreEqual(expectedBytes2, secondAttachment.ContentBytes);
        }

        [TestMethod]
        public void MsGraph_AttachmentConverter_AddAttachments_WithNoAttachments_ShouldReturnEmptyList()
        {
            EmailRequest request = new()
            {
                Attachments = []
            };

            List<Microsoft.Graph.Models.Attachment> attachments = request.AddAttachments();

            Assert.IsNotNull(attachments);
            Assert.IsEmpty(attachments);
        }
    }
}