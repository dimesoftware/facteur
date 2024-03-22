using System;
using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// An email service that can send out emails.
    /// </summary>
    public interface IMailer
    {
        /// <summary>
        /// Sends out an email
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        Task SendMailAsync(EmailRequest request);

        /// <summary>
        /// Sends out an email
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="request">The request</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose);
    }
}