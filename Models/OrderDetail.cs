using MTKPM_FE.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTKPM_FE.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [ForeignKey(nameof(Order))]
        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;

        [ForeignKey(nameof(Product))]
        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
