using System;
using Polly;

namespace Facteur.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder for configuring a mailer's retry policy.
    /// </summary>
    public class MailerConfigurationBuilder
    {
        private readonly FailoverMailerConfiguration _parent;
        private readonly Func<IServiceProvider, IMailer> _factory;

        internal MailerConfigurationBuilder(FailoverMailerConfiguration parent, Func<IServiceProvider, IMailer> factory)
        {
            _parent = parent;
            _factory = factory;
        }

        /// <summary>
        /// Configures the retry policy for this mailer. If not called, the mailer will be tried once without retries.
        /// </summary>
        /// <param name="configurePolicy">The action to configure the retry policy.</param>
        /// <returns>The failover mailer configuration builder for fluent chaining.</returns>
        public FailoverMailerConfiguration WithRetryPolicy(Action<ResiliencePipelineBuilder> configurePolicy)
        {
            ArgumentNullException.ThrowIfNull(configurePolicy);

            ResiliencePipelineBuilder builder = new ResiliencePipelineBuilder();
            configurePolicy(builder);
            ResiliencePipeline policy = builder.Build();

            _parent.AddMailerWithPolicy(_factory, policy);
            return _parent;
        }

        /// <summary>
        /// Adds the mailer without a retry policy. The mailer will be tried once before moving to the next mailer.
        /// </summary>
        /// <returns>The failover mailer configuration builder for fluent chaining.</returns>
        public FailoverMailerConfiguration WithoutRetryPolicy()
        {
            _parent.AddMailerWithPolicy(_factory, policy: null);
            return _parent;
        }
    }
}