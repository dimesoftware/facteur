using System.Collections.Generic;
using Microsoft.Graph.Models;

namespace Facteur.MsGraph
{
    internal static class AttachmentConverter
    {
        internal static IEnumerable<Microsoft.Graph.Models.Attachment> AddAttachments(this EmailRequest request)
        {
            List<FileAttachment> attachments = [];
            foreach (Attachment attachment in request.Attachments)
                attachments.Add(new FileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    Name = attachment.Name,
                    ContentBytes = attachment.ContentBytes
                });

            return attachments;
        }
    }
}