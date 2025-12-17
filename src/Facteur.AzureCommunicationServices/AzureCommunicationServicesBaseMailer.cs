using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;

namespace Facteur.AzureCommunicationServices
{
    /// <summary>
    /// Base class for sending mails using Azure Communication Services
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class AzureCommunicationServicesBaseMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCommunicationServicesBaseMailer"/> class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        protected AzureCommunicationServicesBaseMailer(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null");

            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="request">The email request</param>
        /// <returns></returns>
        public virtual async Task SendMailAsync(EmailRequest request)
        {
            EmailClient client = new(ConnectionString);

            EmailContent content = new(request.Subject) { Html = request.Body };

            EmailMessage message = new(
                senderAddress: request.From.Email,                
                content: content,
                recipients: new EmailRecipients([.. request.To.Select(x => new EmailAddress(x))]));

            message.AddCc(request);
            message.AddBcc(request);
            message.AddAttachments(request);

            try
            {
                EmailSendOperation emailSendOperation = await client.SendAsync(WaitUntil.Completed, message);
                if (emailSendOperation.HasValue && emailSendOperation.Value.Status == EmailSendStatus.Failed)
                    throw new Exception($"Failed to send email via Azure Communication Services. Status: {emailSendOperation.Value.Status}");
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Failed to send email via Azure Communication Services. Error code: {ex.ErrorCode}, Message: {ex.Message}", ex);
            }
        }
    }
}