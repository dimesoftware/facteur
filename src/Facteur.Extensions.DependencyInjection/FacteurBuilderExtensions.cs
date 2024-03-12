using System;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public static class FacteurBuilderExtensions
    {
        public static FacteurBuilder AddFacteur(this IServiceCollection services) 
            => new(services);

        public static IServiceCollection AddFacteur(this IServiceCollection services, Action<FacteurConfiguration> configurator)
        {
            FacteurBuilder builder = new(services);
            configurator(builder);
            return services;
        }
    }
}