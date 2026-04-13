using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;

namespace Facteur.AzureCommunicationServices
{
    /// <summary>
    /// Represents a mail service using Azure Communication Services with plain text content
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureCommunicationServicesPlainTextMailer : AzureCommunicationServicesBaseMailer, IMailer
    {
        private readonly IEmailComposer _composer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCommunicationServicesPlainTextMailer"/> class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="composer">The email composer</param>
        public AzureCommunicationServicesPlainTextMailer(string connectionString, IEmailComposer composer = null)
            : base(connectionString)
        {
            _composer = composer ?? new EmailComposer();
        }

        /// <summary>
        /// Sends out an email with plain text content
        /// </summary>
        /// <param name="request">The email request</param>
        /// <returns>An instance of <see cref="Task"/></returns>
        public override async Task SendMailAsync(EmailRequest request)
        {
            EmailClient client = new(ConnectionString);

            EmailContent content = new(request.Subject)
            {
                PlainText = request.Body
            };

            string senderAddress = !string.IsNullOrEmpty(request.From.Name)
                ? $"{request.From.Name} <{request.From.Email}>"
                : request.From.Email;

            EmailMessage message = new(
                senderAddress: senderAddress,
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

        /// <summary>
        /// Sends the mail asynchronously using the composer pattern
        /// </summary>
        /// <param name="compose">The compose function</param>
        /// <returns></returns>
        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await SendMailAsync(await compose(_composer));
    }
}
