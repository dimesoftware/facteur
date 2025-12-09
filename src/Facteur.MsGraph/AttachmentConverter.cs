using System.Collections.Generic;
using Microsoft.Graph.Models;

namespace Facteur.MsGraph
{
    internal static class AttachmentConverter
    {
        internal static List<Microsoft.Graph.Models.Attachment> AddAttachments(this EmailRequest request)
        {
            List<Microsoft.Graph.Models.Attachment> attachments = [];
            foreach (Attachment attachment in request.Attachments)
                attachments.Add(new FileAttachment
                {
                    Name = attachment.Name,
                    ContentBytes = attachment.ContentBytes
                });

            return attachments;
        }
    }
}