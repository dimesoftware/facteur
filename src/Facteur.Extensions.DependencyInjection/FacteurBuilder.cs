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

        public FacteurBuilder WithMailer<TMailer>(Func<IServiceProvider, TMailer> mailerFactory = null)
            where TMailer : class, IMailer
        {
            if (mailerFactory != null)
                Services.AddScoped<IMailer, TMailer>(mailerFactory);
            else
                Services.AddScoped<IMailer, TMailer>();

            return this;
        }

        public FacteurBuilder WithComposer<TEmailComposer>(Func<IServiceProvider, TEmailComposer> emailComposerFactory = null)
             where TEmailComposer : class, IEmailComposer
        {
            if (emailComposerFactory != null)
                Services.AddScoped<IEmailComposer, TEmailComposer>(emailComposerFactory);
            else
                Services.AddScoped<IEmailComposer, EmailComposer>();

            return this;
        }

        public FacteurBuilder WithTemplatedComposer()
        {
            Services.AddScoped(typeof(IEmailComposer<>), typeof(EmailComposer<>));

            return this;
        }

        public FacteurBuilder WithCompiler<TTemplateCompiler>(Func<IServiceProvider, TTemplateCompiler> templateCompilerFactory = null)
            where TTemplateCompiler : class, ITemplateCompiler
        {
            if (templateCompilerFactory != null)
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>(templateCompilerFactory);
            else
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>();

            return this;
        }

        public FacteurBuilder WithTemplateProvider<TTemplateProvider>(Func<IServiceProvider, TTemplateProvider> templateProviderFactory = null)
            where TTemplateProvider : class, ITemplateProvider
        {
            if (templateProviderFactory != null)
                Services.AddScoped<ITemplateProvider, TTemplateProvider>(templateProviderFactory);
            else
                Services.AddScoped<ITemplateProvider, TTemplateProvider>();

            return this;
        }

        public FacteurBuilder WithResolver<TTemplateResolver>(Func<IServiceProvider, TTemplateResolver> templateResolverFactory = null)
            where TTemplateResolver : class, ITemplateResolver
        {
            if (templateResolverFactory != null)
                Services.AddScoped<ITemplateResolver, TTemplateResolver>(templateResolverFactory);
            else
                Services.AddScoped<ITemplateResolver, TTemplateResolver>();

            return this;
        }
    }
}