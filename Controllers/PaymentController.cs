using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Text.Json;

namespace MTKPM_FE.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _ctx;
        private readonly myContext _db;

        public PaymentController(
            IConfiguration config,
            IHttpContextAccessor ctx,
            myContext db)
        {
            _config = config;
            _ctx = ctx;
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePayment(decimal amount)
        {
            // 1) Tạo Order (Pending)
            var order = new Order
            {
                CreatedAt = DateTime.Now,
                TotalAmount = amount,
                PaymentStatus = "Pending",
                OrderStatus = "New"
            };

            // ← THÊM DÒNG NÀY: gán CustomerId từ session
            var custIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(custIdStr))
                return RedirectToAction("Login", "Account");
            order.CustomerId = int.Parse(custIdStr);

            _db.tbl_order.Add(order);
            _db.SaveChanges();   // order.OrderID sẽ có ở đây

            // 2) Lấy cart JSON từ Session
            var cartJson = _ctx.HttpContext.Session.GetString("cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItemViewModel>()
                : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson)!;

            // --- Debug: kiểm tra có bao nhiêu item trong cart ---
            Console.WriteLine($"[CreatePayment] Cart contains {cart.Count} items.");

            // 3) Lưu từng item vào OrderDetail
            foreach (var item in cart)
            {
                _db.tbl_orderdetail.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }
            _db.SaveChanges();

            // --- Debug: kiểm tra đã ghi bao nhiêu OrderDetail ---
            var countDetails = _db.tbl_orderdetail.Count(d => d.OrderID == order.OrderID);
            Console.WriteLine($"[CreatePayment] Inserted {countDetails} order details.");

            // 4) Build VNPAY payload
            var vnpUrl = _config["VnPay:Url"] ?? throw new Exception("VnPay:Url missing");
            var returnUrl = _config["VnPay:ReturnUrl"] ?? throw new Exception("VnPay:ReturnUrl missing");
            var tmnCode = _config["VnPay:TmnCode"] ?? throw new Exception("VnPay:TmnCode missing");
            var secret = _config["VnPay:HashSecret"] ?? throw new Exception("VnPay:HashSecret missing");

            var ip = _ctx.HttpContext.Connection.RemoteIpAddress!;
            var ipAddr = IPAddress.IsLoopback(ip)
                ? "127.0.0.1"
                : (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString()! : ip.ToString()!);

            long vnpAmount = Convert.ToInt64(Math.Round(amount * 100M, 0));

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = vnpAmount.ToString(CultureInfo.InvariantCulture),
                ["vnp_CurrCode"] = "VND",
                ["vnp_TxnRef"] = order.OrderID.ToString(),
                ["vnp_OrderInfo"] = $"Thanh toan don hang #{order.OrderID}",
                ["vnp_OrderType"] = "other",
                ["vnp_Locale"] = "vn",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_IpAddr"] = ipAddr,
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            var sb = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                sb.Append(WebUtility.UrlEncode(kv.Key))
                  .Append('=')
                  .Append(WebUtility.UrlEncode(kv.Value))
                  .Append('&');
            }
            sb.Length--;
            var payload = sb.ToString();
            var secureHash = ComputeHmacSHA512(secret, payload);

            // 5) Redirect
            return Redirect($"{vnpUrl}?{payload}&vnp_SecureHash={secureHash}");
        }

        [HttpGet]
        public IActionResult PaymentReturn()
        {
            var secret = _config["VnPay:HashSecret"]
                         ?? throw new Exception("Missing VnPay:HashSecret");

            // 1) Thu thập param vnp_*
            var all = HttpContext.Request.Query
                .Where(q => q.Key.StartsWith("vnp_"))
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            // 2) Bắt vnp_SecureHash và loại bỏ trước khi rebuild
            if (!all.TryGetValue("vnp_SecureHash", out var sentHash))
                return Content("Missing vnp_SecureHash");
            all.Remove("vnp_SecureHash");

            // 3) Sort + build rawData
            var sorted = all.OrderBy(kv => kv.Key, StringComparer.Ordinal).ToList();
            var sb = new StringBuilder();
            foreach (var kv in sorted)
            {
                sb.Append(WebUtility.UrlEncode(kv.Key))
                  .Append('=')
                  .Append(WebUtility.UrlEncode(kv.Value))
                  .Append('&');
            }
            sb.Length--;
            var rawData = sb.ToString();

            // 4) Tính HMAC
            var calcHash = ComputeHmacSHA512(secret, rawData);

            // 5) So sánh hash
            if (!string.Equals(calcHash, sentHash, StringComparison.OrdinalIgnoreCase))
                return View("Error");

            // 6) Kiểm tra response code
            all.TryGetValue("vnp_ResponseCode", out var code);
            int orderId1 = 0;
            if (all.TryGetValue("vnp_TxnRef", out var txnRef))
                int.TryParse(txnRef, out orderId1);

            if (code != "00")
            {
                if (orderId1 > 0)
                {
                    var order1 = _db.tbl_order.FirstOrDefault(o => o.OrderID == orderId1);
                    if (order1 != null)
                    {
                        // Sửa tên trường nếu model của bạn dùng tên khác
                        order1.PaymentStatus = "Canceled";
                        order1.OrderStatus = "Canceled";
                        // order.UpdatedAt = DateTime.UtcNow; // nếu có trường này
                        _db.SaveChanges();
                    }
                }

                ViewBag.Message = $"Thanh toán thất bại / bị hủy. Code: {code}";
                ViewBag.VnpData = all;
                return View("PaymentReturn");
            }

            // 7) Thành công → lấy OrderID
            int orderId = int.Parse(all["vnp_TxnRef"]);

            // 8) Load order kèm details
            var order = _db.tbl_order
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                ViewBag.Message = "Không tìm thấy đơn hàng.";
                return View("Error");
            }

            // 9) Cập nhật trạng thái và lưu vào DB
            order.PaymentStatus = "Paid";
            order.OrderStatus = "Processing"; // hoặc "Completed" tuỳ bạn
            _db.SaveChanges();

            // 10) Chuẩn bị view model
            var invoice = new InvoiceViewModel
            {
                OrderID = order.OrderID,
                CreatedAt = order.CreatedAt,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.OrderStatus,
                Items = order.OrderDetails.Select(d => new InvoiceItem
                {
                    ProductName = d.Product.product_name,
                    Quantity = d.Quantity,
                    UnitPrice = d.Price
                }).ToList()
            };

            ViewBag.Message = "Thanh toán thành công!";
            return View("Invoice", invoice);
        }


        private static string ComputeHmacSHA512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        [HttpGet]
        public IActionResult Invoice(int id)
        {
            // Load đơn + details
            var order = _db.tbl_order
                .Include(o => o.OrderDetails)
                  .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            var invoice = new InvoiceViewModel
            {
                OrderID = order.OrderID,
                CreatedAt = order.CreatedAt,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.OrderStatus,
                Items = order.OrderDetails.Select(d => new InvoiceItem
                {
                    ProductName = d.Product.product_name,
                    Quantity = d.Quantity,
                    UnitPrice = d.Price
                }).ToList()
            };

            return View("Invoice", invoice);
        }

    }
}
