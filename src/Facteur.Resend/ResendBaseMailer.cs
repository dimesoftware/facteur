using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Resend;

namespace Facteur.Resend
{
    /// <summary>
    /// Base class for sending mails using Resend
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ResendBaseMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResendBaseMailer"/> class
        /// </summary>
        /// <param name="apiKey">The api key</param>
        protected ResendBaseMailer(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "Api key cannot be null");

            ApiKey = apiKey;
        }

        protected string ApiKey { get; }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="request">The email request</param>
        /// <returns></returns>
        public virtual async Task SendMailAsync(EmailRequest request)
        {
            OptionsSnapshotWrapper<ResendClientOptions> options = new(new ResendClientOptions { ApiToken = ApiKey });
            HttpClient httpClient = new();
            ResendClient client = new(options, httpClient);

            EmailMessage message = new()
            {
                From = new() { Email = request.From.Email, DisplayName = request.From.Name },
                Subject = request.Subject,
                HtmlBody = request.Body
            };

            foreach (string to in request.To)
            {
                message.To.Add(to);
            }
            message.AddCc(request);
            message.AddBcc(request);
            message.AddAttachments(request);

            await client.EmailSendAsync(message);
        }
    }

    [ExcludeFromCodeCoverage]
    internal class OptionsSnapshotWrapper<T> : IOptionsSnapshot<T> where T : class
    {
        private readonly T _value;

        public OptionsSnapshotWrapper(T value)
        {
            _value = value;
        }

        public T Value => _value;

        public T Get(string name) => _value;
    }
}