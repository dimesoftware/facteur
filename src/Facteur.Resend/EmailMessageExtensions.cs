using System.Collections.Generic;
using System.Linq;
using Resend;

namespace Facteur.Resend
{
    internal static class EmailMessageExtensions
    {
        internal static EmailMessage AddCc(this EmailMessage message, EmailRequest request)
        {
            IEnumerable<string> sendCc = request.Cc.Where(x => !request.To.Contains(x));

            foreach (string cc in sendCc)
                message.Cc.Add(cc);

            return message;
        }

        internal static EmailMessage AddBcc(this EmailMessage message, EmailRequest request)
        {
            IEnumerable<string> sendBcc = request.Bcc.Where(x => !request.To.Contains(x) && !request.Cc.Contains(x));
            foreach (string bcc in sendBcc)
                message.Bcc.Add(bcc);

            return message;
        }
    }
}