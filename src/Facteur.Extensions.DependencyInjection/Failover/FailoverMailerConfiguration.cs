using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;

namespace Facteur.Extensions.DependencyInjection
{
    /// <summary>
    /// Configuration builder for failover mailers with individual retry policies.
    /// </summary>
    public class FailoverMailerConfiguration
    {
        private readonly List<MailerConfiguration> _mailerConfigurations = [];

        internal FailoverMailerConfiguration()
        {
        }

        /// <summary>
        /// Adds a mailer to the failover chain. You can optionally configure a retry policy for this mailer.
        /// If no retry policy is configured (using <see cref="MailerConfigurationBuilder.WithoutRetryPolicy"/>), the mailer will be tried once before moving to the next.
        /// </summary>
        /// <typeparam name="TMailer">The mailer implementation type.</typeparam>
        /// <param name="factory">The factory function to create the mailer instance.</param>
        /// <returns>A builder to optionally configure the retry policy for this mailer.</returns>
        public MailerConfigurationBuilder WithMailer<TMailer>(Func<IServiceProvider, TMailer> factory) where TMailer : class, IMailer
        {
            ArgumentNullException.ThrowIfNull(factory);
            return new MailerConfigurationBuilder(this, sp => factory(sp));
        }

        internal FailoverMailer Build(IServiceProvider serviceProvider)
        {
            List<RetryableMailerEntry> mailersWithRetries = [.. _mailerConfigurations
                .Select(config =>
                {
                    IMailer mailer = config.Factory(serviceProvider);

                    // If policy is null, use a pass-through retry function (no retries)
                    Func<Func<Task>, Task> retryFunction = config.Policy == null
                        ? async (func) => await func()
                        : async (func) => await config.Policy.ExecuteAsync(async _ => await func());

                    return new RetryableMailerEntry(mailer, retryFunction);
                })];

            return new FailoverMailer(mailersWithRetries);
        }

        internal void AddMailerWithPolicy(Func<IServiceProvider, IMailer> factory, ResiliencePipeline? policy)
            => _mailerConfigurations.Add(new MailerConfiguration(factory, policy));
    }
}