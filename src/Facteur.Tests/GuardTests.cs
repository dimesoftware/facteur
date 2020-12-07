using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    public class GuardTests
    {
        [TestMethod]
        public void Guard_ThrowIfNullOrEmpty_Collection_IsNotEmpty_ShouldPass()
        {
            Customer customer = new Customer { Name = "Customer #1", Orders = new List<Order>() { new Order() } };
            Guard.ThrowIfNullOrEmpty(customer.Orders, nameof(customer.Orders));

        }
        [TestMethod]
        public void Guard_ThrowIfNullOrEmpty_Collection_IsEmpty_ShouldThrowArgumentNullException()
        {
            Customer customer = new Customer { Name = "Customer #1", Orders = new List<Order>() };
            Assert.ThrowsException<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(customer.Orders, nameof(customer.Orders)));
        }

        [TestMethod]
        public void Guard_ThrowIfNullOrEmpty_Collection_IsNull_ShouldThrowArgumentNullException()
        {
            Customer customer = new Customer { Name = "Customer #1" };
            Assert.ThrowsException<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(customer.Orders, nameof(customer.Orders)));
        }

        [TestMethod]
        public void Guard_ThrowIfNullOrEmpty_Text_IsEmpty_ShouldThrowArgumentNullException()
        {
            Customer customer = new Customer { Name = "" };
            Assert.ThrowsException<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(customer.Name, nameof(customer.Name)));
        }

        [TestMethod]
        public void Guard_ThrowIfNullOrEmpty_Text_IsNull_ShouldThrowArgumentNullException()
        {
            Customer customer = new Customer { };
            Assert.ThrowsException<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(customer.Name, nameof(customer.Name)));
        }

        [TestMethod]
        public void Guard_ThrowIfNullOrEmpty_Text_IsNotNullOrEmpty_ShouldPass()
        {
            Customer customer = new Customer {  Name = "Customer #1" };
            Guard.ThrowIfNullOrEmpty(customer.Name, nameof(customer.Name));
        }
    }
}
