using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection.Resiliency
{
    /// <summary>
    /// Extension methods for FacteurBuilder to add failover mailer support.
    /// </summary>
    public static class FacteurBuilderExtensions
    {
        /// <summary>
        /// Adds multiple mailers with individual retry policies for failover support.
        /// Mailers will be tried in sequence with their configured retry policies before moving to the next.
        /// </summary>
        /// <param name="builder">The FacteurBuilder instance.</param>
        /// <param name="configure">The configuration action to add mailers and configure their retry policies.</param>
        /// <returns>A reference to the builder instance after the operation has completed.</returns>
        public static FacteurBuilder WithMailers(this FacteurBuilder builder, Action<FailoverMailerConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            IServiceCollection services = builder.Services;

            // Remove any existing IMailer registration
            ServiceDescriptor? existingDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMailer));
            if (existingDescriptor != null)
                services.Remove(existingDescriptor);

            FailoverMailerConfiguration config = new();
            configure(config);

            services.AddScoped<IMailer>(config.Build);
            return builder;
        }
    }
}