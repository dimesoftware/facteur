using System.Collections.Generic;

namespace Facteur
{
    public class EmailRequest
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public Sender From { get; set; }

        public IEnumerable<string> To { get; set; } = new List<string>();

        public IEnumerable<string> Cc { get; set; } = new List<string>();

        public IEnumerable<string> Bcc { get; set; } = new List<string>();

        public List<Attachment> Attachments { get; set; } = new();
    }
}