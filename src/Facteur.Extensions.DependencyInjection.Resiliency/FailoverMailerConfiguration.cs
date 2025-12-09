using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;

namespace Facteur.Extensions.DependencyInjection.Resiliency
{
    /// <summary>
    /// Configuration builder for failover mailers with individual retry policies.
    /// </summary>
    public class FailoverMailerConfiguration
    {
        private readonly List<MailerFactoryEntry> _mailerEntries = [];

        internal FailoverMailerConfiguration()
        {
        }

        /// <summary>
        /// Adds a mailer to the failover chain with an optional retry policy configuration.
        /// </summary>
        /// <typeparam name="TMailer">The mailer implementation type.</typeparam>
        /// <param name="factory">The factory function to create the mailer instance.</param>
        /// <param name="configurePolicy">Optional action to configure the retry policy. If null, the mailer will be tried once without retries.</param>
        /// <returns>The failover mailer configuration builder for fluent chaining.</returns>
        public FailoverMailerConfiguration WithMailer<TMailer>(Func<IServiceProvider, TMailer> factory, Action<ResiliencePipelineBuilder>? configurePolicy = null) where TMailer : class, IMailer
        {
            ArgumentNullException.ThrowIfNull(factory);

            ResiliencePipeline? policy = null;
            if (configurePolicy != null)
            {
                ResiliencePipelineBuilder builder = new ResiliencePipelineBuilder();
                configurePolicy(builder);
                policy = builder.Build();
            }

            _mailerEntries.Add(new MailerFactoryEntry(factory, policy));

            return this;
        }

        internal FailoverMailer Build(IServiceProvider serviceProvider)
        {
            List<MailerEntry> mailersWithRetries = [.. _mailerEntries
                .Select(entry =>
                {
                    IMailer mailer = entry.Factory(serviceProvider);

                    // If policy is null, use a pass-through retry function (no retries)
                    Func<Func<Task>, Task> retryFunction = entry.Policy == null ? async (func) => await func() : async (func) => await entry.Policy.ExecuteAsync(async _ => await func());
                    return new MailerEntry(mailer, retryFunction);
                })];

            return new FailoverMailer(mailersWithRetries);
        }
    }
}