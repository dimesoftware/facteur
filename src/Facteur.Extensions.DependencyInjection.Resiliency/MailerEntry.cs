using System;
using System.Threading.Tasks;

namespace Facteur.Extensions.DependencyInjection.Resiliency
{
    /// <summary>
    /// Represents a mailer entry in a failover chain with its associated retry function.
    /// </summary>
    /// <param name="Mailer"> Gets the mailer instance. </param>
    /// <param name="RetryFunction"> Gets the retry function that wraps the mailer call. </param>
    internal record MailerEntry(IMailer Mailer, Func<Func<Task>, Task> RetryFunction)
    {
    }
}