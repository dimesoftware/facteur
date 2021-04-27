using Microsoft.Graph;

namespace Facteur.MsGraph
{
    internal static class AttachmentConverter
    {
        internal static IMessageAttachmentsCollectionPage AddAttachments(this EmailRequest request)
        {
            MessageAttachmentsCollectionPage attachments = new();
            foreach (Attachment attachment in request.Attachments)
                attachments.Add(new FileAttachment
                {
                    ODataType = "#microsoft.graph.fileAttachment",
                    Name = attachment.Name,
                    ContentBytes = attachment.ContentBytes
                });

            return attachments;
        }
    }
}