using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// Represents an e-mail service
    /// </summary>
    public interface IMailer
    {
        /// <summary>
        /// Sends out an e-mail
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        Task SendMailAsync(EmailRequest request);

        /// <summary>
        /// Sends out an e-mail
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <param name="request">The request</param>
        /// <returns>An instance of <see cref="System.Threading.Tasks.Task"/></returns>
        Task SendMailAsync<T>(EmailRequest<T> request) where T : class;
    }
}