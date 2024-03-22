using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Attachments.IO.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileAttachmentTests
    {
        [TestMethod]
        public async Task FileAttachment_Txt_ShouldCreateFileAttachment()
        {
            IAttachmentSource fileAttachment = new FileAttachment();
            Attachment attachment = await fileAttachment.Fetch("Attachments/Attachment.txt");

            Assert.IsTrue(attachment.Name == "Attachment.txt");
            Assert.IsTrue(attachment.ContentBytes.Length == 14);
        }
    }
}