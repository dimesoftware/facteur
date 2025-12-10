#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facteur.Extensions.DependencyInjection
{
    /// <summary>
    /// A composite mailer that tries multiple mailers in sequence.
    /// If one mailer fails, it will attempt to send using the next mailer in the collection.
    /// Only throws an exception if all mailers in the collection fail.
    /// </summary>
    internal class CompositeMailer : IMailer
    {
        private readonly IReadOnlyList<IMailer>? _mailers;
        private readonly IReadOnlyList<MailerEntry>? _entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMailer"/> class.
        /// Mailers will be tried in sequence - if one fails, the next is tried immediately.
        /// </summary>
        /// <param name="mailers">The mailers to compose. They will be tried in the order provided.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mailers"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="mailers"/> is empty.</exception>
        public CompositeMailer(IEnumerable<IMailer> mailers)
        {
            ArgumentNullException.ThrowIfNull(mailers);

            _mailers = mailers.ToList().AsReadOnly();
            _entries = null;

            if (_mailers.Count == 0)
                throw new ArgumentException("At least one mailer must be provided.", nameof(mailers));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMailer"/> class with per-mailer retry functions.
        /// This constructor is used by the Resiliency package to support Polly policies.
        /// </summary>
        /// <param name="mailersWithRetries">The mailer entries with their individual retry functions. The retry function takes a task and returns a task that may retry on failure.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mailersWithRetries"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="mailersWithRetries"/> is empty.</exception>
        internal CompositeMailer(IEnumerable<MailerEntry> mailersWithRetries)
        {
            ArgumentNullException.ThrowIfNull(mailersWithRetries);

            _entries = mailersWithRetries.ToList().AsReadOnly();
            _mailers = null;

            if (_entries.Count == 0)
                throw new ArgumentException("At least one mailer must be provided.", nameof(mailersWithRetries));
        }

        /// <summary>
        /// Sends out an email by trying each mailer in sequence until one succeeds.
        /// </summary>
        /// <param name="request">The email request</param>
        /// <returns>An instance of <see cref="Task"/></returns>
        /// <exception cref="AggregateException">Thrown when all mailers in the failover chain fail. Contains all exceptions from the failed attempts.</exception>
        public async Task SendMailAsync(EmailRequest request)
        {
            List<Exception> exceptions = [];

            if (_entries != null)
            {
                // Use entries with retry functions (from Resiliency package)
                foreach (MailerEntry entry in _entries)
                {
                    try
                    {
                        await entry.RetryFunction(() => entry.Mailer.SendMailAsync(request));
                        return;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }
            else if (_mailers != null)
            {
                // Simple try/catch failover (standard DI package)
                foreach (IMailer mailer in _mailers)
                {
                    try
                    {
                        await mailer.SendMailAsync(request);
                        return;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            throw new AggregateException("All mailers in the collection failed to send the email.", exceptions);
        }

        /// <summary>
        /// Sends out an email by trying each mailer in sequence until one succeeds.
        /// </summary>
        /// <param name="compose">The function that composes the email request</param>
        /// <returns>An instance of <see cref="Task"/></returns>
        /// <exception cref="AggregateException">Thrown when all mailers in the failover chain fail. Contains all exceptions from the failed attempts.</exception>
        public async Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
        {
            List<Exception> exceptions = [];

            if (_entries != null)
            {
                // Use entries with retry functions (from Resiliency package)
                foreach (MailerEntry entry in _entries)
                {
                    try
                    {
                        await entry.RetryFunction(() => entry.Mailer.SendMailAsync(compose));
                        return;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }
            else if (_mailers != null)
            {
                // Simple try/catch failover (standard DI package)
                foreach (IMailer mailer in _mailers)
                {
                    try
                    {
                        await mailer.SendMailAsync(compose);
                        return;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            throw new AggregateException("All mailers in the collection failed to send the email.", exceptions);
        }
    }
}
