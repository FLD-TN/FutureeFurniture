using MTKPM_FE.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MTKPM_FE.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string OrderStatus { get; set; } = "New";

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
