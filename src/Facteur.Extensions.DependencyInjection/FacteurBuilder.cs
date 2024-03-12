using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public class FacteurConfiguration
    {
        public FacteurConfiguration(IServiceCollection services)
        {
            Services = services;
        }

        protected IServiceCollection Services { get; }

        public FacteurConfiguration WithMailer<TMailer>(Func<IServiceProvider, TMailer> mailerFactory = null)
            where TMailer : class, IMailer
        {
            if (mailerFactory != null)
                Services.AddScoped<IMailer, TMailer>(mailerFactory);
            else
                Services.AddScoped<IMailer, TMailer>();

            return this;
        }

        public FacteurConfiguration WithCompiler<TTemplateCompiler>(Func<IServiceProvider, TTemplateCompiler> templateCompilerFactory = null)
            where TTemplateCompiler : class, ITemplateCompiler
        {
            if (templateCompilerFactory != null)
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>(templateCompilerFactory);
            else
                Services.AddScoped<ITemplateCompiler, TTemplateCompiler>();

            return this;
        }

        public FacteurConfiguration WithTemplateProvider<TTemplateProvider>(Func<IServiceProvider, TTemplateProvider> templateProviderFactory = null)
            where TTemplateProvider : class, ITemplateProvider
        {
            if (templateProviderFactory != null)
                Services.AddScoped<ITemplateProvider, TTemplateProvider>(templateProviderFactory);
            else
                Services.AddScoped<ITemplateProvider, TTemplateProvider>();

            return this;
        }

        public FacteurConfiguration WithResolver<TTemplateResolver>(Func<IServiceProvider, TTemplateResolver> templateResolverFactory = null)
            where TTemplateResolver : class, ITemplateResolver
        {
            if (templateResolverFactory != null)
                Services.AddScoped<ITemplateResolver, TTemplateResolver>(templateResolverFactory);
            else
                Services.AddScoped<ITemplateResolver, TTemplateResolver>();

            return this;
        }
    }

    public class FacteurBuilder : FacteurConfiguration, IServiceCollection
    {
        public FacteurBuilder(IServiceCollection services) : base(services)
        {
        }

        int ICollection<ServiceDescriptor>.Count => Services.Count;

        bool ICollection<ServiceDescriptor>.IsReadOnly => Services.IsReadOnly;

        ServiceDescriptor IList<ServiceDescriptor>.this[int index]
        {
            get => Services[index];
            set => Services[index] = value;
        }

        int IList<ServiceDescriptor>.IndexOf(ServiceDescriptor item)
            => Services.IndexOf(item);

        void IList<ServiceDescriptor>.Insert(int index, ServiceDescriptor item)
            => Services.Insert(index, item);

        void IList<ServiceDescriptor>.RemoveAt(int index)
            => Services.RemoveAt(index);

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
            => Services.Add(item);

        void ICollection<ServiceDescriptor>.Clear()
            => Services.Clear();

        bool ICollection<ServiceDescriptor>.Contains(ServiceDescriptor item)
            => Services.Contains(item);

        void ICollection<ServiceDescriptor>.CopyTo(ServiceDescriptor[] array, int arrayIndex)
            => Services.CopyTo(array, arrayIndex);

        bool ICollection<ServiceDescriptor>.Remove(ServiceDescriptor item)
            => Services.Remove(item);

        IEnumerator<ServiceDescriptor> IEnumerable<ServiceDescriptor>.GetEnumerator()
            => Services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Services.GetEnumerator();
    }
}