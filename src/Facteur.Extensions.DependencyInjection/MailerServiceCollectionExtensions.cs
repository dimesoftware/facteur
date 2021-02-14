using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class MailerServiceCollectionExtensions
    {
        public static IServiceCollection AddMailer<TMailer, TTemplateCompiler, TTemplateProvider, TTemplateResolver>(
            this IServiceCollection services,
            Func<IServiceProvider, TMailer> mailerFactory = null,
            Func<IServiceProvider, TTemplateCompiler> templateCompilerFactory = null,
            Func<IServiceProvider, TTemplateProvider> templateProviderFactory = null,
            Func<IServiceProvider, TTemplateResolver> templateResolverFactory = null)
            where TMailer : class, IMailer
            where TTemplateCompiler : class, ITemplateCompiler
            where TTemplateProvider : class, ITemplateProvider
            where TTemplateResolver : class, ITemplateResolver
        {
            if (mailerFactory != null)
                services.AddScoped<IMailer, TMailer>(mailerFactory);
            else
                services.AddScoped<IMailer, TMailer>();

            if (templateCompilerFactory != null)
                services.AddScoped<ITemplateCompiler, TTemplateCompiler>(templateCompilerFactory);
            else
                services.AddScoped<ITemplateCompiler, TTemplateCompiler>();

            if (templateProviderFactory != null)
                services.AddScoped<ITemplateProvider, TTemplateProvider>(templateProviderFactory);
            else
                services.AddScoped<ITemplateProvider, TTemplateProvider>();

            if (templateResolverFactory != null)
                services.AddScoped<ITemplateResolver, TTemplateResolver>(templateResolverFactory);
            else
                services.AddScoped<ITemplateResolver, TTemplateResolver>();

            return services;
        }
    }
}