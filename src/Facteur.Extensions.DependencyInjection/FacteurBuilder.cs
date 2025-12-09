using System;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    /// <summary>
    /// The builder class responsible for hooking up Facteur's modules to the runtime.
    /// </summary>
    public class FacteurBuilder
    {
        public FacteurBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }

        /// <summary>
        /// Adds the mailer to the runtime.
        /// </summary>
        /// <typeparam name="TMailer">The mailer implementation, such as SMTP, MS Graph, etc.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public FacteurBuilder WithMailer<TMailer>(Func<IServiceProvider, TMailer> implementationFactory = null)
            where TMailer : class, IMailer
        {
            if (implementationFactory != null)
                Services.AddScoped<IMailer, TMailer>(implementationFactory);
            else
                Services.AddScoped<IMailer, TMailer>();

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