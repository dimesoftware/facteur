using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using t = System.Threading.Tasks;

namespace Facteur.MsGraph
{
    /// <summary>
    /// Mail component that uses SendGrid as the transport and RazorEngine as the content builder
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GraphMailer : IMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMailer"/> class
        /// </summary>
        /// <param name="credentials"></param>
        public GraphMailer(GraphCredentials credentials)
        {
            Credentials = credentials;
        }

        private GraphCredentials Credentials { get; }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns></returns>
        public async t.Task SendMailAsync(EmailRequest request)
        {
            GraphServiceClient graphClient = await ConnectClient().ConfigureAwait(false);
            Message message = new Message
            {
                Subject = request.Subject,
                Body = new ItemBody { ContentType = BodyType.Html, Content = request.Body },
                ToRecipients = request.To.Select(x => new Recipient { EmailAddress = new EmailAddress { Address = x } }),
                Attachments = request.AddAttachments()
            };

            await graphClient.Users[Credentials.From]
                .SendMail(message, false)
                .Request()
                .PostAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async t.Task SendMailAsync<T>(EmailRequest<T> request) where T : class
        {
            GraphServiceClient graphClient = await ConnectClient().ConfigureAwait(false);

            Message message = new Message
            {
                Subject = request.Subject,
                Body = new ItemBody { ContentType = BodyType.Html, Content = request.Body },
                ToRecipients = request.To.Select(x => new Recipient { EmailAddress = new EmailAddress { Address = x } }),
                CcRecipients = request.Cc.Select(x => new Recipient { EmailAddress = new EmailAddress { Address = x } }),
                BccRecipients = request.Bcc.Select(x => new Recipient { EmailAddress = new EmailAddress { Address = x } }),
                Attachments = request.AddAttachments()
            };

            await graphClient.Users[Credentials.From]
                .SendMail(message, false)
                .Request()
                .PostAsync()
                .ConfigureAwait(false);
        }

        protected async t.Task<GraphServiceClient> ConnectClient()
        {
            string[] scopes = { "https://graph.microsoft.com/.default" };
            string authority = $"https://login.microsoftonline.com/{Credentials.TenantId}";

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(Credentials.ClientId)
                .WithTenantId(Credentials.TenantId)
                .WithClientSecret(Credentials.ClientSecret)
                .WithAuthority(authority)
                .Build();

            AuthenticationResult authenticationResult = await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);

            return new GraphServiceClient(new DelegateAuthenticationProvider(x =>
            {
                x.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                return t.Task.FromResult(0);
            }));
        }
    }
}