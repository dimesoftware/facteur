using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Facteur.AzureCommunicationServices
{
    /// <summary>
    /// Mail component that uses Azure Communication Services as the transport
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureCommunicationServicesMailer : AzureCommunicationServicesBaseMailer, IMailer
    {
        private readonly IEmailComposer _composer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCommunicationServicesMailer"/> class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="composer">The email composer</param>
        public AzureCommunicationServicesMailer(string connectionString, IEmailComposer composer = null)
            : base(connectionString)
        {
            _composer = composer ?? new EmailComposer();
        }

        /// <summary>
        /// Sends the mail asynchronously using the composer pattern
        /// </summary>
        /// <param name="compose">The compose function</param>
        /// <returns></returns>
        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
            => await base.SendMailAsync(await compose(_composer));
    }
}
