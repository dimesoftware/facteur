using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Facteur.Plunk
{
    /// <summary>
    /// Mail component that uses Plunk as the transport
    /// </summary>
    public class PlunkMailer : IMailer
    {
        private readonly IEmailComposer _composer;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiEndpoint = "https://api.useplunk.com/v1/send";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlunkMailer"/> class
        /// </summary>
        /// <param name="apiKey">The Plunk API key</param>
        /// <param name="composer">The email composer</param>
        /// <param name="httpClient">Optional HttpClient instance</param>
        public PlunkMailer(string apiKey, IEmailComposer composer = null, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "API key cannot be null or empty");

            _apiKey = apiKey;
            _composer = composer ?? new EmailComposer();
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public async Task SendMailAsync(EmailRequest request)
        {
            // Combine all recipients into the "to" field as Plunk API accepts array of recipients
            List<string> allRecipients = new();

            if (request.To != null)
                allRecipients.AddRange(request.To);
            
            if (request.Cc != null)
                allRecipients.AddRange(request.Cc.Where(x => !allRecipients.Contains(x)));
            
            if (request.Bcc != null)
                allRecipients.AddRange(request.Bcc.Where(x => !allRecipients.Contains(x)));

            // Build payload dictionary
            Dictionary<string, object> payload = new()
            {
                { "to", allRecipients.Count == 1 ? (object)allRecipients[0] : allRecipients },
                { "subject", request.Subject },
                { "body", request.Body }
            };

            // Add attachments if present
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                var attachments = request.Attachments.Select(att => new Dictionary<string, object>
                {
                    { "name", att.Name },
                    { "content", Convert.ToBase64String(att.ContentBytes) }
                }).ToArray();

                payload["attachments"] = attachments;
            }

            string jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using HttpRequestMessage requestMessage = new(HttpMethod.Post, ApiEndpoint)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="compose">The compose function</param>
        /// <returns></returns>
        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await SendMailAsync(await compose(_composer));
    }
}

