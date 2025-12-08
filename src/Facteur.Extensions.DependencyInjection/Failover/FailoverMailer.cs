using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facteur.Extensions.DependencyInjection
{
    /// <summary>
    /// A mailer that provides failover support by trying multiple mailers in sequence.
    /// If one mailer fails, it will attempt to send using the next mailer in the failover chain.
    /// Only throws an exception if all mailers in the failover chain fail.
    /// </summary>
    internal class FailoverMailer : IMailer
    {
        private readonly IReadOnlyList<RetryableMailerEntry> _entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverMailer"/> class.
        /// Mailers will be tried in sequence without retries - if one fails, the next is tried immediately.
        /// </summary>
        /// <param name="mailers">The mailers to configure for failover. They will be tried in the order provided.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mailers"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="mailers"/> is empty.</exception>
        public FailoverMailer(IEnumerable<IMailer> mailers)
        {
            ArgumentNullException.ThrowIfNull(mailers);

            List<IMailer> mailersList = [.. mailers];

            if (mailersList.Count == 0)
                throw new ArgumentException("At least one mailer must be provided.", nameof(mailers));

            // Convert to entries with pass-through retry functions (no retries)
            _entries = mailersList
                .Select(mailer => new RetryableMailerEntry(mailer, async (func) => await func()))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverMailer"/> class with per-mailer retry functions.
        /// Each mailer can have a custom retry strategy before moving to the next mailer.
        /// Mailers without retry policies can use a pass-through retry function that doesn't retry.
        /// </summary>
        /// <param name="mailersWithRetries">The mailer entries with their individual retry functions. The retry function takes a task and returns a task that may retry on failure.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mailersWithRetries"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="mailersWithRetries"/> is empty.</exception>
        public FailoverMailer(IEnumerable<RetryableMailerEntry> mailersWithRetries)
        {
            ArgumentNullException.ThrowIfNull(mailersWithRetries);

            _entries = mailersWithRetries.ToList().AsReadOnly();

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

            foreach (RetryableMailerEntry entry in _entries)
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

            throw new AggregateException("All mailers in the failover chain failed to send the email.", exceptions);
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

            foreach (RetryableMailerEntry entry in _entries)
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

            throw new AggregateException("All mailers in the failover chain failed to send the email.", exceptions);
        }
    }
}