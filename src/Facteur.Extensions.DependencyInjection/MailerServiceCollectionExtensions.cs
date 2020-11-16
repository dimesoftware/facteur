using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public static class MailerServiceCollectionExtensions
    {
        public static IServiceCollection AddMailer<TMailer>(this IServiceCollection services)
        where TMailer : class, IMailer
        {
            services.AddScoped<IMailer, TMailer>();

            return services;
        }
    }
}