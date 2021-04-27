using System;
using System.Linq;
using SendGrid.Helpers.Mail;

namespace Facteur.SendGrid
{
    internal static class AttachmentConverter
    {
        internal static SendGridMessage AddAttachments(this SendGridMessage message, EmailRequest request)
        {
            message.Attachments = request.Attachments.Select(x => new global::SendGrid.Helpers.Mail.Attachment
            {
                Content = Convert.ToBase64String(x.ContentBytes),
                Filename = x.Name,
                Disposition = "attachment"
            }).ToList();

            return message;
        }
    }
}