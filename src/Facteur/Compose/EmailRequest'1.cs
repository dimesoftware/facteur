namespace Facteur
{
    public class EmailRequest<T> : EmailRequest
    {
        public T Model { get; set; }

        public EmailRequest<T> Copy(string body = null)
            => new()
            {
                Subject = Subject,
                Body = body ?? Body,
                From = From,
                To = To,
                Cc = Cc,
                Bcc = Bcc,
                Attachments = Attachments
            };
    }
}