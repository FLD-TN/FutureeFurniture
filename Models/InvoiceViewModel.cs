using System;
using System.Collections.Generic;
using MTKPM_FE.Models;

namespace MTKPM_FE.Models
{
    public class InvoiceViewModel
    {
        public int OrderID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentStatus { get; set; } = "";
        public string OrderStatus { get; set; } = "";
        public List<InvoiceItem> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(i => i.LineTotal);
    }

    public class InvoiceItem
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
