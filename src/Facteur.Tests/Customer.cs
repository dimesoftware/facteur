using System.Collections.Generic;

namespace Facteur.Tests
{
    public class Customer
    {
        public string Name { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }
}