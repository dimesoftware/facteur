#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    /// <summary>
    /// The builder class responsible for hooking up Facteur's modules to the runtime.
    /// </summary>
    public class FacteurBuilder
    {
        private readonly List<Func<IServiceProvider, IMailer>> _mailerFactories = [];

        public FacteurBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }

        /// <summary>
        /// Adds a mailer to the runtime. When multiple mailers are added, they are automatically wrapped in a CompositeMailer
        /// that will try each mailer in sequence until one succeeds.
        /// </summary>
        /// <typeparam name="TMailer">The mailer implementation, such as SMTP, MS Graph, etc.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithMailer<TMailer>(Func<IServiceProvider, TMailer>? implementationFactory = null)
            where TMailer : class, IMailer
        {
            // Add the mailer factory to our collection
            if (implementationFactory != null)
                _mailerFactories.Add(implementationFactory);
            else
            {
                // For type-based registration, we need to ensure the type is registered and create a factory that resolves it
                // Only add if not already registered to avoid overwriting existing registrations
                if (!Services.Any(s => s.ServiceType == typeof(TMailer)))
                    Services.AddScoped<TMailer>();

                _mailerFactories.Add(sp => sp.GetRequiredService<TMailer>());
            }

            // Remove any existing IMailer registration
            ServiceDescriptor? existingDescriptor = Services.FirstOrDefault(s => s.ServiceType == typeof(IMailer));
            if (existingDescriptor != null)
                Services.Remove(existingDescriptor);

            // Register the mailer(s) based on count
            // Single mailer - register directly
            // Multiple mailers - wrap in CompositeMailer
            if (_mailerFactories.Count == 1)
                Services.AddScoped<IMailer>(_mailerFactories[0]);
            else
                Services.AddScoped<IMailer>(serviceProvider => new CompositeMailer([.. _mailerFactories.Select(factory => factory(serviceProvider))]));

            return this;
        }

        /// <summary>
        /// Adds a composer to the runtime
        /// </summary>
        /// <typeparam name="TEmailComposer">The composer that constructs and populates email requests.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithComposer<TEmailComposer>(Func<IServiceProvider, TEmailComposer> implementationFactory = null)
             where TEmailComposer : class, IEmailComposer
        {
            if (implementationFactory != null)
                Services.AddScoped<IEmailComposer, TEmailComposer>(implementationFactory);
            else
                Services.AddScoped<IEmailComposer, EmailComposer>();

            return this;
        }

        /// <summary>
        /// Adds the default composer to the runtime. Requires that ITemplateCompiler, ITemplateProvider, ITemplateResolver can be resolved.
        /// </summary>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithDefaultComposer()
        {
            Services.AddScoped<IEmailComposer, EmailComposer>();
            return this;
        }

        /// <summary>
        /// Adds a template compiler to the runtime, such as Scriban, RazorEngine, etc.
        /// </summary>
        /// <typeparam name="TTemplateCompiler">The type of the implementation to use.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithCompiler<TTemplateCompiler>(Func<IServiceProvider, TTemplateCompiler> implementationFactory = null)
            where TTemplateCompiler : class, ITemplateCompiler
        {
            if (implementationFactory != null)
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>(implementationFactory);
            else
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>();

            return this;
        }

        /// <summary>
        /// Adds a template provider to the runtime, such as Azure Blobs, local storage, e tc.
        /// </summary>
        /// <typeparam name="TTemplateProvider">The type of the implementation to use.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithTemplateProvider<TTemplateProvider>(Func<IServiceProvider, TTemplateProvider> implementationFactory = null)
            where TTemplateProvider : class, ITemplateProvider
        {
            if (implementationFactory != null)
                Services.AddScoped<ITemplateProvider, TTemplateProvider>(implementationFactory);
            else
                Services.AddScoped<ITemplateProvider, TTemplateProvider>();

            return this;
        }

        /// <summary>
        /// Adds a template resolver to the runtime, responsible for location the right file for the email body POCO type.
        /// </summary>
        /// <typeparam name="TTemplateResolver">The type of the implementation to use.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithResolver<TTemplateResolver>(Func<IServiceProvider, TTemplateResolver> implementationFactory = null)
            where TTemplateResolver : class, ITemplateResolver
        {
            if (implementationFactory != null)
                Services.AddScoped<ITemplateResolver, TTemplateResolver>(implementationFactory);
            else
                Services.AddScoped<ITemplateResolver, TTemplateResolver>();

            return this;
        }
    }
}