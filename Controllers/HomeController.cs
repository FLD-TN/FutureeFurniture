﻿using MTKPM_FE.Helpers;
using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MTKPM_FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly myContext _context;

        public HomeController(myContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy 8 sản phẩm mới nhất
            var products = await _context.tbl_product.OrderByDescending(p => p.CreatedAt).Take(8).ToListAsync();

            // Lấy 4 bài blog mới nhất để hiển thị trên trang chủ
            ViewBag.News = await _context.tbl_blog.OrderByDescending(b => b.CreatedAt).Take(4).ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> AllProducts(int? categoryId, string categorySlug, string sortOrder, int? page)
        {
            // Bắt đầu với một IQueryable cơ bản
            var productsQuery = _context.tbl_product.AsNoTracking();

            // Ưu tiên lọc theo categoryId nếu có
            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.cat_id == categoryId.Value);
                ViewBag.CurrentCategory = categoryId;
            }
            // Nếu không có categoryId, xét đến categorySlug (từ link "Sản phẩm mới")
            else if (!string.IsNullOrEmpty(categorySlug))
            {
                if (categorySlug.ToLower() == "sanphammoi")
                {
                    // Đây là logic cũ của bạn để lọc sản phẩm mới
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    ViewBag.CurrentSlug = categorySlug; // Gửi slug để View biết
                    // Gán sortOrder để View biết và hiển thị đúng trạng thái
                    sortOrder = "newest";
                }
                // Bạn có thể thêm các slug khác ở đây nếu cần
                // else if (categorySlug == "khuyenmai") { ... }
            }

            // Áp dụng logic sắp xếp dựa trên sortOrder
            ViewBag.CurrentSort = sortOrder;
            switch (sortOrder)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.product_discount_price ?? p.product_price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.product_discount_price ?? p.product_price);
                    break;
                case "rating_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.product_rating);
                    break;
                case "newest": // Đã được xử lý ở trên, chỉ để switch nhận diện
                    break;
                default: // Mặc định hoặc khi không có sortOrder
                    productsQuery = productsQuery.OrderBy(p => p.product_name);
                    break;
            }

            // Thiết lập các thông số phân trang
            int pageSize = 8; // Số sản phẩm trên mỗi trang (bạn có thể đổi thành 8, 16, 20, v.v.)
            int pageNumber = (page ?? 1); // Nếu page là null thì mặc định là trang 1

            // Tạo PaginatedList từ query và thông số phân trang
            ViewBag.Categories = await _context.tbl_category.ToListAsync(); // Gửi danh sách category cho View

            var paginatedProducts = await PaginatedList<Product>.CreateAsync(productsQuery, pageNumber, pageSize);

            return View(paginatedProducts);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage contactMessage)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var customerIdSession = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdSession))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                // và lưu lại URL trang liên hệ để quay lại sau khi đăng nhập thành công
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để gửi tin nhắn.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Contact", "Home") });
            }

            if (ModelState.IsValid)
            {
                // Lấy thông tin khách hàng từ session
                var customerId = int.Parse(customerIdSession);
                var customer = await _context.tbl_customer.FindAsync(customerId);

                // Tạo một đối tượng Feedback để lưu tin nhắn
                var feedback = new Feedback
                {
                    // Lấy tên và email từ thông tin khách hàng đã đăng nhập
                    user_name = customer?.customer_name ?? contactMessage.Name,
                    user_message = contactMessage.Message,
                    Content = contactMessage.Message, // Lưu nội dung tin nhắn
                    Rating = 0, // Đánh dấu là tin nhắn liên hệ (không phải đánh giá sản phẩm)
                    CreatedAt = DateTime.Now,
                    CustomerId = customerId // Gán ID của khách hàng đã đăng nhập
                };

                _context.tbl_feedback.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi tin nhắn. Chúng tôi sẽ phản hồi sớm nhất!";
                return RedirectToAction("Contact");
            }

            // Nếu model không hợp lệ, hiển thị lại form với các lỗi
            return View(contactMessage);
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _context.tbl_product.FindAsync(id);
            if (product == null)
            {
                return NotFound(); // Trả về trang 404 nếu không tìm thấy sản phẩm
            }

            // Lấy danh sách sản phẩm đã xem gần đây từ session
            var recentlyViewed = HttpContext.Session.Get<List<int>>("RecentlyViewed") ?? new List<int>();

            // Thêm sản phẩm hiện tại vào đầu danh sách (nếu chưa có)
            if (!recentlyViewed.Contains(id))
            {
                recentlyViewed.Insert(0, id);
            }

            // Giới hạn danh sách chỉ 5 sản phẩm
            var limitedList = recentlyViewed.Take(5).ToList();
            HttpContext.Session.Set("RecentlyViewed", limitedList);

            // Lấy thông tin chi tiết của các sản phẩm đã xem (trừ sản phẩm hiện tại)
            var suggestedProducts = await _context.tbl_product
                                        .Where(p => limitedList.Contains(p.product_id) && p.product_id != id)
                                        .ToListAsync();

            ViewBag.RecentlyViewedProducts = suggestedProducts;
            return View(product);
        }

        // Action để hiển thị trang danh sách tất cả bài blog (có phân trang)
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<Product>());
            }

            var products = await _context.tbl_product
                .Where(p => p.product_name.ToLower().Contains(q.ToLower()))
                .Take(10) // Giới hạn 10 kết quả để tối ưu
                .Select(p => new
                {
                    p.product_id,
                    p.product_name,
                    p.product_image,
                    p.product_price,
                    p.product_discount_price
                })
                .ToListAsync();

            return Json(products);
        }
    }
}