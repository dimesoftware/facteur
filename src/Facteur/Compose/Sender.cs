namespace Facteur
{
    public struct Sender
    {
        public Sender(string email, string name)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}