using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTKPM_FE.Controllers
{
    public class OrdersController : Controller
    {
        
        private readonly myContext _db; // Đổi tên thành _context để nhất quán
        public OrdersController(myContext db)
        {
            _db = db;
        }

        // GET: /Orders
        public IActionResult Index()
        {
            // Giả sử bạn đã lưu customerId vào Session khi login:
            var custJson = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(custJson))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(custJson);

            // Lấy tất cả đơn của customer này
            var orders = _db.tbl_order
                .Where(o => o.CustomerId == customerId)    // Sửa thành CustomerId cho khớp model
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrderCOD()
        {
            var custIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(custIdStr) || !int.TryParse(custIdStr, out var customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.Get<List<CartItemViewModel>>("cart") ?? new List<CartItemViewModel>();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                CustomerId = customerId,
                CreatedAt = DateTime.Now,
                TotalAmount = cart.Sum(item => item.Total),
                PaymentStatus = "Unpaid",
                OrderStatus = "Pending Confirmation",
                OrderDetails = new List<OrderDetail>()
            };

            // Thêm chi tiết đơn hàng
            foreach (var cartItem in cart)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductID = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price
                });
            }

            _db.tbl_order.Add(order);
            await _db.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đã đặt hàng thành công
            HttpContext.Session.Remove("cart");

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _db.tbl_order.FindAsync(id);
            if (order == null) return NotFound();
            
            return View(order);
        }
    }
}
