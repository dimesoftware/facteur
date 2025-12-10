#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public static class FacteurBuilderExtensions
    {
        /// <summary>
        /// Adds Facteur, the email composition kit, to the service collection.
        /// </summary>
        /// <returns>An instance of the FacteurServiceCollection class.</returns>
        public static FacteurServiceCollection AddFacteur(this IServiceCollection services)
            => new(services);

        /// <summary>
        /// Adds Facteur, the email composition kit, to the service collection.
        /// </summary>
        /// <param name="services">The current service collection</param>
        /// <param name="configure">The builder method</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddFacteur(this IServiceCollection services, Action<FacteurBuilder> configure)
        {
            FacteurServiceCollection builder = new(services);
            configure(builder);
            return services;
        }

    }
}