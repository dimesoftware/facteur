using System;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public class FacteurBuilder
    {
        public FacteurBuilder(IServiceCollection services)
        {
            Services = services;
        }

        protected IServiceCollection Services { get; }

        public FacteurBuilder WithMailer<TMailer>(Func<IServiceProvider, TMailer> implementationFactory = null)
            where TMailer : class, IMailer
        {
            if (implementationFactory != null)
                Services.AddScoped<IMailer, TMailer>(implementationFactory);
            else
                Services.AddScoped<IMailer, TMailer>();

            return this;
        }

        public FacteurBuilder WithComposer<TEmailComposer>(Func<IServiceProvider, TEmailComposer> implementationFactory = null)
             where TEmailComposer : class, IEmailComposer
        {
            if (implementationFactory != null)
                Services.AddScoped<IEmailComposer, TEmailComposer>(implementationFactory);
            else
                Services.AddScoped<IEmailComposer, EmailComposer>();

            return this;
        }

        public FacteurBuilder WithTemplatedComposer()
        {
            Services.AddScoped(typeof(IEmailComposer<>), typeof(EmailComposer<>));

            return this;
        }

        public FacteurBuilder WithCompiler<TTemplateCompiler>(Func<IServiceProvider, TTemplateCompiler> implementationFactory = null)
            where TTemplateCompiler : class, ITemplateCompiler
        {
            if (implementationFactory != null)
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>(implementationFactory);
            else
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>();

            return this;
        }

        public FacteurBuilder WithTemplateProvider<TTemplateProvider>(Func<IServiceProvider, TTemplateProvider> implementationFactory = null)
            where TTemplateProvider : class, ITemplateProvider
        {
            if (implementationFactory != null)
                Services.AddScoped<ITemplateProvider, TTemplateProvider>(implementationFactory);
            else
                Services.AddScoped<ITemplateProvider, TTemplateProvider>();

            return this;
        }

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