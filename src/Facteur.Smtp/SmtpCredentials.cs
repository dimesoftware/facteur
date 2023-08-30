namespace Facteur.Smtp
{
    public readonly struct SmtpCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpCredentials"/> class
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="defaultCredentials"></param>
        /// <param name="enableSsl"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public SmtpCredentials(string host, string port, string defaultCredentials, string enableSsl)
        {
            Host = host;
            Port = int.Parse(port);

            bool.TryParse(enableSsl, out bool shouldEnableSsl);
            EnableSsl = shouldEnableSsl;

            bool.TryParse(defaultCredentials, out bool useDefaultCredentials);
            UseDefaultCredentials = useDefaultCredentials;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpCredentials"/> class
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="defaultCredentials"></param>
        /// <param name="enableSsl"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public SmtpCredentials(string host, string port, string defaultCredentials, string enableSsl, string email, string password)
            : this(host, port, defaultCredentials, enableSsl)
        {
            Email = email;
            Password = password;
        }

        public string Email { get; }
        public string Password { get; }
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public bool UseDefaultCredentials { get; }
    }
}