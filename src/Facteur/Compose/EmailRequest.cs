using System.Collections.Generic;

namespace Facteur
{
    /// <summary>
    /// The POCO type that contains all the information of an email.
    /// </summary>
    public class EmailRequest
    {
        public string Subject { get; set; }

        public string Body { get; set; }

        public Sender From { get; set; }

        public IEnumerable<string> To { get; set; } = [];

        public IEnumerable<string> Cc { get; set; } = [];

        public IEnumerable<string> Bcc { get; set; } = [];

        public List<Attachment> Attachments { get; set; } = [];
    }
}