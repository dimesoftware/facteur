using System;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public static class FacteurBuilderExtensions
    {
        public static FacteurServiceCollection AddFacteur(this IServiceCollection services)
            => new(services);

        public static IServiceCollection AddFacteur(this IServiceCollection services, Action<FacteurBuilder> configure)
        {
            FacteurServiceCollection builder = new(services);
            configure(builder);
            return services;
        }
    }
}