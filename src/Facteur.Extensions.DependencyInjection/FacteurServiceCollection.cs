using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Facteur.Extensions.DependencyInjection
{
    public class FacteurServiceCollection : FacteurBuilder, IServiceCollection
    {
        public FacteurServiceCollection(IServiceCollection services) : base(services)
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