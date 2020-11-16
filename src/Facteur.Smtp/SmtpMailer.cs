using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Facteur.Smtp
{
    /// <summary>
    /// Mail component that uses SMTP as the transport and RazorEngine as the content builder
    /// </summary>
    public class SmtpMailer : IMailer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailer"/> class
        /// </summary>
        /// <param name="smtpCredentials"></param>
        public SmtpMailer(SmtpCredentials smtpCredentials)
            : this(smtpCredentials, new ViewModelTemplateResolver(), new RazorEngineTemplateCompiler())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailer"/> class
        /// </summary>
        /// <param name="smtpCredentials">The SendGrid API key</param>
        /// <param name="templateResolver"></param>
        /// <param name="compiler"></param>
        public SmtpMailer(SmtpCredentials smtpCredentials, ITemplateResolver templateResolver, ITemplateCompiler compiler)
        {
            Credentials = smtpCredentials;

            Resolver = templateResolver;
            Compiler = compiler;
        }

        protected SmtpCredentials Credentials { get; }
        private ITemplateResolver Resolver { get; }
        private ITemplateCompiler Compiler { get; }

        /// <summary>
        /// Sends the mail asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task SendMailAsync(EmailRequest request)
        {
            using SmtpClient smtpClient = new SmtpClient(Credentials.Host)
            {
                Port = Credentials.Port,
                Credentials = new NetworkCredential(Credentials.Email, Credentials.Password),
                EnableSsl = Credentials.EnableSsl
            };

            if (Credentials.UseDefaultCredentials)
                smtpClient.UseDefaultCredentials = Credentials.UseDefaultCredentials;

            using MailMessage msg = new MailMessage();
            msg.Subject = request.Subject;
            msg.Body = request.Body;
            msg.IsBodyHtml = true;
            msg.From = new MailAddress(request.From);

            if (request.To != null && request.To.Any())
                msg.To.Add(string.Join(",", request.To));

            if (request.Cc != null && request.Cc.Any())
                msg.CC.Add(string.Join(",", request.Cc));

            if (request.Bcc != null && request.Bcc.Any())
                msg.Bcc.Add(string.Join(",", request.Bcc));

            await smtpClient.SendMailAsync(msg).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the mail asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task SendMailAsync<T>(EmailRequest<T> request)
            where T : class
        {
            request.Body = await Compiler.CompileBody(request.Model, Resolver.Resolve(request.Model)).ConfigureAwait(false);
            await SendMailAsync((EmailRequest)request).ConfigureAwait(false);
        }
    }
}