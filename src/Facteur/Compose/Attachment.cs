namespace Facteur
{
    public class Attachment
    {
        public Attachment()
        {
        }

        public Attachment(string name, byte[] contentBytes)
        {
            Name = name;
            ContentBytes = contentBytes;
        }

        public string Name { get; set; }
        public byte[] ContentBytes { get; set; }
    }
}