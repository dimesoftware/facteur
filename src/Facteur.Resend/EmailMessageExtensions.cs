using System.Collections.Generic;
using System.Linq;
using Resend;

namespace Facteur.Resend
{
    internal static class EmailMessageExtensions
    {
        internal static EmailMessage AddCc(this EmailMessage message, EmailRequest request)
        {
            if (request.Cc == null || !request.Cc.Any())
                return message;

            IEnumerable<string> sendCc = request.Cc.Where(x => !request.To.Contains(x));

            foreach (string cc in sendCc)
            {
                if (message.Cc == null)
                    message.Cc = new EmailAddressList();
                message.Cc.Add(cc);
            }

            return message;
        }

        internal static EmailMessage AddBcc(this EmailMessage message, EmailRequest request)
        {
            if (request.Bcc == null || !request.Bcc.Any())
                return message;

            IEnumerable<string> sendBcc = request.Bcc.Where(x => !request.To.Contains(x) && !request.Cc.Contains(x));
            foreach (string bcc in sendBcc)
            {
                if (message.Bcc == null)
                    message.Bcc = new EmailAddressList();
                message.Bcc.Add(bcc);
            }

            return message;
        }
    }
}