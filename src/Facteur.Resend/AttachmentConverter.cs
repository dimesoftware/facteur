using System.Linq;
using Resend;

namespace Facteur.Resend
{
    internal static class AttachmentConverter
    {
        internal static EmailMessage AddAttachments(this EmailMessage message, EmailRequest request)
        {
            if (request.Attachments.Count == 0)
                return message;

            message.Attachments = [.. request.Attachments.Select(x => new EmailAttachment
            {
                Content = x.ContentBytes,
                Filename = x.Name
            })];

            return message;
        }
    }
}