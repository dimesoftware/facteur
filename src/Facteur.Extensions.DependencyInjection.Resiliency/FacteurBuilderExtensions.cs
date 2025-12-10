#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facteur.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Facteur.Extensions.DependencyInjection.Resiliency
{
    /// <summary>
    /// Extension methods for FacteurBuilder to add resilient mailer support with Polly policies.
    /// </summary>
    public static class FacteurBuilderExtensions
    {
        private static readonly Dictionary<FacteurBuilder, List<ResilientMailerEntry>> _resilientMailers = [];

        extension(FacteurBuilder builder)
        {
            /// <summary>
            /// Adds a mailer with an optional Polly retry policy. When multiple mailers are added, they are automatically
            /// wrapped in a CompositeMailer that will try each mailer in sequence with their configured retry policies.
            /// This overload allows specifying Polly policies for each mailer to manage retry behavior.
            /// </summary>
            /// <typeparam name="TMailer">The mailer implementation type.</typeparam>
            /// <param name="factory">The factory function to create the mailer instance.</param>
            /// <param name="configurePolicy">Optional action to configure the retry policy. If null, the mailer will be tried once without retries.</param>
            /// <returns>A reference to the builder instance after the operation has completed.</returns>
            public FacteurBuilder WithMailer<TMailer>(Func<IServiceProvider, TMailer>? factory = null, Action<ResiliencePipelineBuilder>? configurePolicy = null)
                where TMailer : class, IMailer
            {
                IServiceCollection services = builder.Services;

                // Get or create the resilient mailers list for this builder instance
                List<ResilientMailerEntry> resilientMailers = _resilientMailers.GetValueOrDefault(builder) ?? (_resilientMailers[builder] = []);

                // Handle factory - if null, use type-based registration
                Func<IServiceProvider, IMailer> mailerFactory;
                if (factory != null)
                    mailerFactory = factory;
                else
                {
                    // For type-based registration, we need to ensure the type is registered and create a factory that resolves it
                    // Only add if not already registered to avoid overwriting existing registrations
                    if (!services.Any(s => s.ServiceType == typeof(TMailer)))
                        services.AddScoped<TMailer>();
                    mailerFactory = sp => sp.GetRequiredService<TMailer>();
                }

                // Build the policy if provided
                ResiliencePipeline? policy = null;
                if (configurePolicy != null)
                {
                    ResiliencePipelineBuilder policyBuilder = new();
                    configurePolicy(policyBuilder);
                    policy = policyBuilder.Build();
                }

                // Add to resilient mailers collection
                resilientMailers.Add(new ResilientMailerEntry(mailerFactory, policy));

                // Remove any existing IMailer registration
                ServiceDescriptor? existingDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMailer));
                if (existingDescriptor != null)
                    services.Remove(existingDescriptor);

                // Register the mailer(s) based on count
                if (resilientMailers.Count == 1)
                {
                    // Single mailer - register directly with optional retry policy
                    ResilientMailerEntry entry = resilientMailers[0];
                    services.AddScoped<IMailer>(serviceProvider =>
                    {
                        IMailer mailer = entry.Factory(serviceProvider);
                        if (entry.Policy == null)
                            return mailer;

                        // Wrap in a composite with retry function
                        Func<Func<Task>, Task> retryFunction = async (func) => await entry.Policy.ExecuteAsync(async _ => await func());
                        MailerEntry mailerEntry = new(mailer, retryFunction);
                        return new CompositeMailer([mailerEntry]);
                    });

                    return builder;
                }

                // Multiple mailers - wrap in CompositeMailer with retry functions
                services.AddScoped<IMailer>(serviceProvider =>
                {
                    List<MailerEntry> mailersWithRetries = [.. resilientMailers.Select(entry =>
                    {
                        IMailer mailer = entry.Factory(serviceProvider);

                        // If policy is null, use a pass-through retry function (no retries)
                        Func<Func<Task>, Task> retryFunction = entry.Policy == null
                            ? async (func) => await func()
                            : async (func) => await entry.Policy.ExecuteAsync(async _ => await func());

                        return new MailerEntry(mailer, retryFunction);
                    })];

                    return new CompositeMailer(mailersWithRetries);
                });

                return builder;
            }
        }

        private record ResilientMailerEntry(Func<IServiceProvider, IMailer> Factory, ResiliencePipeline? Policy);
    }
}