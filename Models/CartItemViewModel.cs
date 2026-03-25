using MTKPM_FE.Models;
namespace MTKPM_FE.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Price { get; set; } // Giá cuối cùng để tính tiền
        public int Quantity { get; set; }

        // Thuộc tính này sẽ tự động tính thành tiền cho mỗi sản phẩm
        public int Total => Quantity * Price;
    }
}
