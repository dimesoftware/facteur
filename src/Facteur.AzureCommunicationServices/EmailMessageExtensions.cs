using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Communication.Email;

namespace Facteur.AzureCommunicationServices
{
    internal static class EmailMessageExtensions
    {
        internal static EmailMessage AddCc(this EmailMessage message, EmailRequest request)
        {
            if (request.Cc == null || !request.Cc.Any())
                return message;

            IEnumerable<string> sendCc = request.Cc.Where(x => !request.To.Contains(x));

            foreach (string cc in sendCc)
                message.Recipients.CC.Add(new EmailAddress(cc));

            return message;
        }

        internal static EmailMessage AddBcc(this EmailMessage message, EmailRequest request)
        {
            if (request.Bcc == null || !request.Bcc.Any())
                return message;

            IEnumerable<string> sendBcc = request.Bcc.Where(x => !request.To.Contains(x) && !request.Cc.Contains(x));
            foreach (string bcc in sendBcc)
                message.Recipients.BCC.Add(new EmailAddress(bcc));

            return message;
        }

        internal static EmailMessage AddAttachments(this EmailMessage message, EmailRequest request)
        {
            if (request.Attachments.Count == 0)
                return message;

            foreach (Attachment attachment in request.Attachments)
            {
                BinaryData binary = new(attachment.ContentBytes);
                message.Attachments.Add(new EmailAttachment(name: attachment.Name, contentType: "application/octet-stream", content: binary));
            }

            return message;
        }
    }
}