using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string _apiEndpoint;
        private const string DefaultBaseUrl = "https://api.useplunk.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlunkMailer"/> class
        /// </summary>
        /// <param name="apiKey">The Plunk API key</param>
        /// <param name="composer">The email composer</param>
        /// <param name="httpClient">Optional HttpClient instance</param>
        /// <param name="baseUrl">Optional base URL for self-hosted Plunk instances. Defaults to https://api.useplunk.com for hosted Plunk</param>
        public PlunkMailer(string apiKey, IEmailComposer composer = null, HttpClient httpClient = null, string baseUrl = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "API key cannot be null or empty");

            _apiKey = apiKey;
            _composer = composer ?? new EmailComposer();
            _httpClient = httpClient ?? new HttpClient();

            // For self-hosted: baseUrl should be like "https://plunk.example.com/api"
            // For hosted: use default "https://api.useplunk.com"
            string baseUri = string.IsNullOrEmpty(baseUrl) ? DefaultBaseUrl : baseUrl.TrimEnd('/');
            _apiEndpoint = $"{baseUri}/v1/send";
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
                Dictionary<string, object>[] attachments = [.. request.Attachments.Select(att => new Dictionary<string, object>
                {
                    { "filename", att.Name },
                    { "content", Convert.ToBase64String(att.ContentBytes) },
                    { "contentType", GetContentType(att.Name) }
                })];

                payload["attachments"] = attachments;
            }

            string jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using HttpRequestMessage requestMessage = new(HttpMethod.Post, _apiEndpoint) { Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json") };
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

        /// <summary>
        /// Gets the MIME content type based on file extension
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The MIME content type</returns>
        private static string GetContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "application/octet-stream";

            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }
}