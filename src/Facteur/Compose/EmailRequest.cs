using System.Collections.Generic;

namespace Facteur
{

    public class EmailRequest
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public Sender From { get; set; }

        public IEnumerable<string> To { get; set; }

        public IEnumerable<string> Cc { get; set; }

        public IEnumerable<string> Bcc { get; set; }

        public List<Attachment> Attachments { get; set; } = new();
    }
}