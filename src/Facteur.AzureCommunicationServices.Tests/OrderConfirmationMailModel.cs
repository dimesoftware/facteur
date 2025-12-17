using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Facteur.Tests
{
    /// <summary>
    /// Advanced view model for demonstrating Scriban template features
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OrderConfirmationMailModel
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZipCode { get; set; }
        public string ShippingCountry { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public string TrackingUrl { get; set; }
        public bool IsPriorityShipping { get; set; }
        public string DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class OrderItem
    {
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImageUrl { get; set; }
    }
}
