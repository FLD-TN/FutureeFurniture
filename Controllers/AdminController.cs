﻿using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MTKPM_FE.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;


    namespace MTKPM_FE.Controllers
    {
        public class AdminController : Controller
        {
            private readonly myContext _context;
            private readonly IWebHostEnvironment _env;

            public AdminController(myContext context, IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }

            // GET: /Admin
            // AdminController.cs
            public IActionResult Index()
            {
                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                // Gán tiêu đề cho trang
                ViewData["Title"] = "Dashboard";

                var now = DateTime.Now;
                var startOfYear = new DateTime(now.Year, 1, 1);
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Thống kê cơ bản
                ViewBag.ProductCount = _context.tbl_product.Count();
                ViewBag.CategoryCount = _context.tbl_category.Count();
                ViewBag.CustomerCount = _context.tbl_customer.Count();
                ViewBag.OrderCount = _context.tbl_order.Count();

                // Doanh thu
                ViewBag.YearlyRevenue = _context.tbl_order
                    .Where(o => o.CreatedAt >= startOfYear && o.PaymentStatus == "Paid")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                ViewBag.MonthlyRevenue = _context.tbl_order
                    .Where(o => o.CreatedAt >= startOfMonth && o.PaymentStatus == "Paid")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                // Số lượng sản phẩm đã bán
                ViewBag.TotalProductsSold = _context.tbl_product.Sum(p => p.product_sold);

                // Dữ liệu biểu đồ doanh thu theo tháng
                var revenueByMonth = _context.tbl_order
                    .Where(o => o.CreatedAt.Year == now.Year) // Lọc theo năm hiện tại
                    .GroupBy(o => o.CreatedAt.Month)       // Nhóm theo tháng ngay trên DB
                    .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) })
                    .OrderBy(r => r.Month)
                    .ToDictionary(r => r.Month, r => r.Total);

                var monthlyLabels = new List<string>();
                var monthlyData = new List<decimal>();

                for (int i = 1; i <= 12; i++)
                {
                    monthlyLabels.Add($"Tháng {i}");
                    monthlyData.Add(revenueByMonth.ContainsKey(i) ? revenueByMonth[i] : 0);
                }

                ViewBag.MonthlyLabels = monthlyLabels;
                ViewBag.MonthlyData = monthlyData;

                return View();
            }

            public IActionResult ViewFeedback(string searchTerm, int? rating,
                DateTime? fromDate, DateTime? toDate)
            {
                var query = _context.tbl_feedback
                    .Include(f => f.Customer)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(f =>
                        f.Content.Contains(searchTerm) ||
                        (f.Customer != null && f.Customer.customer_name.Contains(searchTerm)) || // Tìm theo tên khách hàng
                        (f.Customer == null && f.user_name.Contains(searchTerm)) // Tìm theo tên người gửi liên hệ
                    );
                }

                // Lọc theo đánh giá
                if (rating.HasValue)
                {
                    query = query.Where(f => f.Rating == rating);
                }

                // Lọc theo ngày
                if (fromDate.HasValue)
                {
                    query = query.Where(f => f.CreatedAt >= fromDate);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(f => f.CreatedAt <= toDate);
                }

                var feedbacks = query.OrderByDescending(f => f.CreatedAt).ToList();
                return View(feedbacks);
            }

            // GET: /Admin/ViewContactMessages
            public async Task<IActionResult> ViewContactMessages()
            {
                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                ViewData["Title"] = "Tin nhắn liên hệ";
                var messages = await _context.tbl_contact_message.OrderByDescending(m => m.CreatedAt).ToListAsync();
                return View(messages);
            }

            // GET: /Admin/Logout
            public IActionResult Logout()
            {
                HttpContext.Session.Remove("admin_session");
                return RedirectToAction("Login", "Account");
            }

            // GET: /Admin/Profile
            [HttpGet]
            public IActionResult Profile()
            {
                if (!int.TryParse(HttpContext.Session.GetString("admin_session"), out var id))
                    return RedirectToAction("Login", "Account");

                var admin = _context.tbl_admin.Find(id);
                if (admin == null) return NotFound();

                return View(admin);
            }

            // POST: /Admin/Profile
            [HttpPost]
            public IActionResult Profile(Admin admin)
            {
                var existing = _context.tbl_admin.Find(admin.admin_id);
                if (existing == null) return NotFound();

                existing.admin_name = admin.admin_name;
                existing.admin_email = admin.admin_email;
                // ... cập nhật các trường khác nếu cần ...
                _context.tbl_admin.Update(existing);
                _context.SaveChanges();

                return View("Profile");
            }

            // POST: /Admin/ChangeProfileImage
            [HttpPost]
            public IActionResult ChangeProfileImage(IFormFile admin_image)
            {
                if (!int.TryParse(HttpContext.Session.GetString("admin_session"), out var id))
                    return RedirectToAction("Profile");

                var existing = _context.tbl_admin.Find(id);
                if (existing == null) return NotFound();

                if (admin_image?.Length > 0)
                {
                    var fn = Path.GetFileName(admin_image.FileName);
                    var path = Path.Combine(_env.WebRootPath, "admin_image", fn);
                    using var fs = new FileStream(path, FileMode.Create);
                    admin_image.CopyTo(fs);
                    existing.admin_image = fn;
                    _context.tbl_admin.Update(existing);
                    _context.SaveChanges();
                }

                return RedirectToAction("Profile");
            }

            // --- CUSTOMER MANAGEMENT ---

            // GET: /Admin/FetchCustomer
            public IActionResult FetchCustomer(string searchTerm, string sortBy,
        DateTime? fromDate, DateTime? toDate)
            {
                var query = _context.tbl_customer
                    .Include(c => c.Orders)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c =>
                        c.customer_name.Contains(searchTerm) ||
                        c.customer_email.Contains(searchTerm) ||
                        c.customer_phone.Contains(searchTerm)
                    );
                }

                // Lọc theo ngày đăng ký
                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= fromDate);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= toDate);
                }

                // Sắp xếp
                switch (sortBy)
                {
                    case "name":
                        query = query.OrderBy(c => c.customer_name);
                        break;
                    case "orders":
                        query = query.OrderByDescending(c => c.Orders.Count);
                        break;
                    case "date":
                        query = query.OrderByDescending(c => c.CreatedAt);
                        break;
                    default:
                        query = query.OrderBy(c => c.customer_id);
                        break;
                }

                var customers = query.ToList();
                return View(customers);
            }

            // GET: /Admin/CustomerDetails/{id}
            public IActionResult CustomerDetails(int id)
            {
                var customer = _context.tbl_customer.Find(id);
                if (customer == null) return NotFound();
                return View(customer);
            }

            // GET: /Admin/UpdateCustomer/{id}
            [HttpGet]
            public IActionResult UpdateCustomer(int id)
            {
                var customer = _context.tbl_customer.Find(id);
                if (customer == null) return NotFound();
                return View(customer);
            }

            // POST: /Admin/UpdateCustomer
            [HttpPost]
            public IActionResult UpdateCustomer(Customer customer, IFormFile customer_image)
            {
                var existing = _context.tbl_customer.Find(customer.customer_id);
                if (existing == null) return NotFound();

                // Cập nhật các trường
                existing.customer_name = customer.customer_name;
                existing.customer_phone = customer.customer_phone;
                existing.customer_email = customer.customer_email;
                existing.customer_password = customer.customer_password;
                existing.customer_gender = customer.customer_gender;
                existing.customer_country = customer.customer_country;
                existing.customer_city = customer.customer_city;
                existing.customer_address = customer.customer_address;

                if (customer_image?.Length > 0)
                {
                    var fn = Path.GetFileName(customer_image.FileName);
                    var path = Path.Combine(_env.WebRootPath, "customer_images", fn);
                    using var fs = new FileStream(path, FileMode.Create);
                    customer_image.CopyTo(fs);
                    existing.customer_image = fn;
                }

                _context.tbl_customer.Update(existing);
                _context.SaveChanges();
                return RedirectToAction("FetchCustomer");
            }

            // GET: /Admin/DeletePermissionCustomer/{id}
            [HttpGet]
            public IActionResult DeletePermissionCustomer(int id)
            {
                var cust = _context.tbl_customer.Find(id);
                if (cust == null) return NotFound();
                return View(cust);
            }

            // POST: /Admin/DeleteCustomer
            [HttpPost, ActionName("DeleteCustomer")]
            public IActionResult DeleteCustomerConfirmed(int id)
            {
                var existing = _context.tbl_customer.Find(id);
                if (existing == null) return NotFound();

                _context.tbl_customer.Remove(existing);
                _context.SaveChanges();
                return RedirectToAction("FetchCustomer");
            }

            // --- CATEGORY MANAGEMENT ---

            // GET: /Admin/FetchCategory
            public IActionResult FetchCategory(string searchTerm, string sortBy)
            {
                var query = _context.tbl_category
                    .Include(c => c.Product)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c =>
                        c.category_name.Contains(searchTerm)
                    );
                }

                // Sắp xếp
                switch (sortBy)
                {
                    case "name":
                        query = query.OrderBy(c => c.category_name);
                        break;
                    case "products":
                        query = query.OrderByDescending(c => c.Product.Count);
                        break;
                    default:
                        query = query.OrderBy(c => c.category_id);
                        break;
                }

                var categories = query.ToList();
                return View(categories);
            }

            // GET: /Admin/AddCategory
            [HttpGet]
            public IActionResult AddCategory()
                => View();

            // POST: /Admin/AddCategory
            [HttpPost]
            public IActionResult AddCategory(Category cat)
            {
                _context.tbl_category.Add(cat);
                _context.SaveChanges();
                return RedirectToAction("FetchCategory");
            }

            // GET: /Admin/UpdateCategory/{id}
            [HttpGet]
            public IActionResult UpdateCategory(int id)
            {
                var cat = _context.tbl_category.Find(id);
                if (cat == null) return NotFound();
                return View(cat);
            }

            // POST: /Admin/UpdateCategory
            [HttpPost]
            public IActionResult UpdateCategory(Category cat)
            {
                var existing = _context.tbl_category.Find(cat.category_id);
                if (existing == null) return NotFound();

                existing.category_name = cat.category_name;
                _context.tbl_category.Update(existing);
                _context.SaveChanges();
                return RedirectToAction("FetchCategory");
            }

            // GET: /Admin/DeletePermissionCategory/{id}
            [HttpGet]
            public IActionResult DeletePermissionCategory(int id)
            {
                var cat = _context.tbl_category.Find(id);
                if (cat == null) return NotFound();
                return View(cat);
            }

            // POST: /Admin/DeleteCategory
            [HttpPost, ActionName("DeleteCategory")]
            public IActionResult DeleteCategoryConfirmed(int id)
            {
                var existing = _context.tbl_category.Find(id);
                if (existing == null) return NotFound();

                _context.tbl_category.Remove(existing);
                _context.SaveChanges();
                return RedirectToAction("FetchCategory");
            }

            // --- PRODUCT MANAGEMENT ---

            // GET: /Admin/FetchProduct
            public IActionResult FetchProduct(string searchTerm, int? categoryId,
                decimal? minPrice, decimal? maxPrice, string sortBy)
            {
                var query = _context.tbl_product
                    .Include(p => p.Category)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p =>
                        p.product_name.Contains(searchTerm) ||
                        p.product_description.Contains(searchTerm)
                    );
                }

                // Lọc theo danh mục
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.cat_id == categoryId);
                }

                // Lọc theo giá
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.product_price >= minPrice);
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.product_price <= maxPrice);
                }

                // Sắp xếp
                switch (sortBy)
                {
                    case "price_asc":
                        query = query.OrderBy(p => p.product_price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(p => p.product_price);
                        break;
                    case "name":
                        query = query.OrderBy(p => p.product_name);
                        break;
                    case "sold":
                        query = query.OrderByDescending(p => p.product_sold);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                var products = query.ToList();
                ViewBag.Categories = _context.tbl_category.ToList();
                return View(products);
            }

            // GET: /Admin/AddProduct
            [HttpGet]
            public IActionResult AddProduct()
            {
                ViewData["category"] = _context.tbl_category.ToList();
                return View();
            }

            // POST: /Admin/AddProduct
            [HttpPost]
            public IActionResult AddProduct(IFormFile product_image, List<IFormFile> product_images)
            {
                var f = Request.Form;
                var prod = new Product
                {
                    product_name = f["product_name"],
                    product_price = int.Parse(f["product_price"]),
                    product_description = f["product_description"],
                    cat_id = int.Parse(f["cat_id"]),
                    product_discount_price = string.IsNullOrEmpty(f["product_discount_price"])
                                            ? (int?)null
                                            : int.Parse(f["product_discount_price"]),
                    product_rating = double.TryParse(f["product_rating"], out var r) ? r : 0,
                    product_review_count = int.TryParse(f["product_review_count"], out var rc) ? rc : 0,
                    product_sold = int.TryParse(f["product_sold"], out var s) ? s : 0,
                    CreatedAt = DateTime.Now
                };

                if (product_image?.Length > 0)
                {
                    var fn = Path.GetFileName(product_image.FileName);
                    var path = Path.Combine(_env.WebRootPath, "product_images", fn);
                    using var fs = new FileStream(path, FileMode.Create);
                    product_image.CopyTo(fs);
                    prod.product_image = fn;
                }

                _context.tbl_product.Add(prod);
                _context.SaveChanges();

                if (product_images?.Any() == true)
                {
                    foreach (var img in product_images.Where(i => i.Length > 0))
                    {
                        var fn = Path.GetFileName(img.FileName);
                        var path = Path.Combine(_env.WebRootPath, "product_images", fn);
                        using var fs2 = new FileStream(path, FileMode.Create);
                        img.CopyTo(fs2);

                        _context.ProductImage.Add(new ProductImage
                        {
                            ProductId = prod.product_id,
                            ImagePath = fn
                        });
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction("FetchProduct");
            }

            // GET: /Admin/ProductDetails/{id}
            public IActionResult ProductDetails(int id)
            {
                var prod = _context.tbl_product
                                .Include(p => p.Category)
                                .Include(p => p.ProductImages)
                                .FirstOrDefault(p => p.product_id == id);

                if (prod == null) return NotFound();
                return View(prod);
            }

            // GET: /Admin/UpdateProduct/{id}
            [HttpGet]
            public IActionResult UpdateProduct(int id)
            {
                ViewData["category"] = _context.tbl_category.ToList();
                var prod = _context.tbl_product
                                .Include(p => p.ProductImages)
                                .FirstOrDefault(p => p.product_id == id);

                if (prod == null) return NotFound();

                ViewBag.selectedCategoryId = prod.cat_id;
                return View(prod);
            }

            // POST: /Admin/UpdateProduct
            [HttpPost]
            public IActionResult UpdateProduct(Product product, List<IFormFile> product_images)
            {
                var existing = _context.tbl_product
                                    .Include(p => p.ProductImages)
                                    .FirstOrDefault(p => p.product_id == product.product_id);

                if (existing == null) return NotFound();

                existing.product_name = product.product_name;
                existing.product_price = product.product_price;
                existing.product_description = product.product_description;
                existing.cat_id = product.cat_id;
                existing.product_discount_price = product.product_discount_price;
                existing.product_rating = product.product_rating;
                existing.product_review_count = product.product_review_count;
                existing.product_sold = product.product_sold;

                if (product_images?.Any() == true)
                {
                    foreach (var img in product_images.Where(i => i.Length > 0))
                    {
                        var fn = Path.GetFileName(img.FileName);
                        var path = Path.Combine(_env.WebRootPath, "product_images", fn);
                        using var fs = new FileStream(path, FileMode.Create);
                        img.CopyTo(fs);

                        _context.ProductImage.Add(new ProductImage
                        {
                            ProductId = existing.product_id,
                            ImagePath = fn
                        });
                    }
                }

                _context.tbl_product.Update(existing);
                _context.SaveChanges();

                return RedirectToAction("FetchProduct");
            }

            // POST: /Admin/DeleteProductImage
            [HttpPost]
            public IActionResult DeleteProductImage(int imageId, int productId)
            {
                var img = _context.ProductImage.Find(imageId);
                if (img != null)
                {
                    var filePath = Path.Combine(_env.WebRootPath, "product_images", img.ImagePath);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    _context.ProductImage.Remove(img);
                    _context.SaveChanges();
                }
                return RedirectToAction("UpdateProduct", new { id = productId });
            }

            // POST: /Admin/ChangeProductImage
            [HttpPost]
            public IActionResult ChangeProductImage(IFormFile product_image, Product product)
            {
                var existing = _context.tbl_product.Find(product.product_id);
                if (existing == null) return NotFound();

                if (product_image?.Length > 0)
                {
                    var fn = Path.GetFileName(product_image.FileName);
                    var path = Path.Combine(_env.WebRootPath, "product_images", fn);
                    using var fs = new FileStream(path, FileMode.Create);
                    product_image.CopyTo(fs);

                    existing.product_image = fn;
                    _context.tbl_product.Update(existing);
                    _context.SaveChanges();
                }
                return RedirectToAction("FetchProduct");
            }

            // GET: /Admin/DeletePermissionProduct/5
            [HttpGet]
            public IActionResult DeletePermissionProduct(int id)
            {
                var prod = _context.tbl_product.Find(id);
                if (prod == null) return NotFound();
                return View(prod);
            }

            // POST: /Admin/DeleteProduct/5
            [HttpPost, ActionName("DeleteProduct")]
            public IActionResult DeleteProductConfirmed(int id)
            {
                var prod = _context.tbl_product.Find(id);
                if (prod != null)
                {
                    _context.tbl_product.Remove(prod);
                    _context.SaveChanges();
                }
                return RedirectToAction("FetchProduct");
            }

            // GET: /Admin/Orders
            [HttpGet]
            public IActionResult Orders(string searchTerm, DateTime? fromDate, DateTime? toDate, decimal? minTotal, decimal? maxTotal)
            {
                // Lấy orders kèm Customer + chi tiết, hỗ trợ tìm kiếm và lọc nâng cao
                var query = _context.tbl_order
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // search by customer name or order id
                    query = query.Where(o => o.Customer.customer_name.Contains(searchTerm) || o.OrderID.ToString().Contains(searchTerm));
                }

                if (fromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= toDate.Value);

                if (minTotal.HasValue)
                    query = query.Where(o => o.TotalAmount >= minTotal.Value);
                if (maxTotal.HasValue)
                    query = query.Where(o => o.TotalAmount <= maxTotal.Value);

                var orders = query.OrderByDescending(o => o.CreatedAt).ToList();

                return View("Orders", orders);
            }

            // GET: /Admin/OrderDetails/{id}
            [HttpGet]
            public IActionResult OrderDetails(int id)
            {
                // Lấy đơn theo id, kèm Customer + chi tiết
                var order = _context.tbl_order
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefault(o => o.OrderID == id);

                if (order == null)
                    return NotFound();

                return View("OrderDetails", order);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async System.Threading.Tasks.Task<IActionResult> ConfirmOrder(int orderId)
            {
                var order = await _context.tbl_order.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound();
                }

                // Cập nhật trạng thái đơn hàng và thanh toán
                // Đối với đơn COD, khi admin xác nhận, ta coi như đơn hàng sẽ được giao và thu tiền thành công.
                order.OrderStatus = "Completed";
                order.PaymentStatus = "Paid";

                _context.tbl_order.Update(order);
                await _context.SaveChangesAsync();

                // Quay lại trang chi tiết đơn hàng để xem trạng thái đã cập nhật
                return RedirectToAction("OrderDetails", new { id = orderId });
            }

            // --- BLOG MANAGEMENT ---

            // GET: /Admin/ManageBlogs
            // Action để hiển thị danh sách các bài blog đã tạo
            public async Task<IActionResult> ManageBlogs()
            {
                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                ViewData["Title"] = "Quản lý Blog";
                var blogs = await _context.tbl_blog.OrderByDescending(b => b.CreatedAt).ToListAsync();
                return View(blogs); // Cần tạo View Views/Admin/ManageBlogs.cshtml
            }

            // GET: /Admin/CreateBlog
            // Action để hiển thị form tạo bài viết mới
            public IActionResult CreateBlog()
            {
                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                ViewData["Title"] = "Tạo bài viết mới";
                return View(); // Trả về View Views/Admin/CreateBlog.cshtml
            }

            // POST: /Admin/CreateBlog
            // Action để xử lý dữ liệu từ form và lưu bài viết
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateBlog(Blog blog, IFormFile photo)
            {
                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                ViewData["Title"] = "Tạo bài viết mới";
                if (ModelState.IsValid)
                {
                    // Xử lý upload hình ảnh đại diện
                    if (photo != null && photo.Length > 0)
                    {
                        // Tạo tên file độc nhất để tránh trùng lặp
                        var fileName = Path.GetFileNameWithoutExtension(photo.FileName);
                        var fileExtension = Path.GetExtension(photo.FileName);
                        var uniqueFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";

                        // Đường dẫn để lưu file (wwwroot/blog_images)
                        var path = Path.Combine(_env.WebRootPath, "blog_images", uniqueFileName);

                        // Đảm bảo thư mục tồn tại
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                        // Copy file vào thư mục
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        // Lưu tên file vào model
                        blog.blog_image = uniqueFileName;
                    }

                    // Tự động tạo slug từ tiêu đề
                    blog.slug = GenerateSlug(blog.blog_title);

                    blog.CreatedAt = DateTime.Now;
                    _context.tbl_blog.Add(blog);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đã tạo bài viết thành công!";
                    return RedirectToAction("ManageBlogs");
                }

                return View(blog);
            }

            // GET: /Admin/EditBlog/5
            // Action để hiển thị form chỉnh sửa bài viết
            public async Task<IActionResult> EditBlog(int? id)
            {
                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                if (id == null)
                {
                    return NotFound();
                }

                var blog = await _context.tbl_blog.FindAsync(id);
                if (blog == null)
                {
                    return NotFound();
                }
                ViewData["Title"] = "Chỉnh sửa bài viết";
                return View(blog); // Cần tạo View Views/Admin/EditBlog.cshtml
            }

            // POST: /Admin/EditBlog/5
            // Action để xử lý việc cập nhật bài viết
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> EditBlog(int id, Blog blog, IFormFile photo)
            {
                if (id != blog.blog_id)
                {
                    return NotFound();
                }

                if (HttpContext.Session.GetString("admin_session") is null)
                    return RedirectToAction("Login", "Account");

                ViewData["Title"] = "Chỉnh sửa bài viết";
                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingBlog = await _context.tbl_blog.FindAsync(id);
                        if (existingBlog == null) return NotFound();

                        // Cập nhật thông tin
                        existingBlog.blog_title = blog.blog_title;
                        existingBlog.blog_description = blog.blog_description;

                        // Tạo lại slug nếu tiêu đề thay đổi hoặc slug đang rỗng
                        existingBlog.slug = GenerateSlug(blog.blog_title);

                        // Xử lý upload ảnh mới nếu có
                        if (photo != null && photo.Length > 0)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(photo.FileName);
                            var fileExtension = Path.GetExtension(photo.FileName);
                            var uniqueFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
                            var path = Path.Combine(_env.WebRootPath, "blog_images", uniqueFileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }
                            // (Tùy chọn) Xóa ảnh cũ nếu cần
                            // ...
                            existingBlog.blog_image = uniqueFileName;
                        }

                        _context.Update(existingBlog);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.tbl_blog.Any(e => e.blog_id == blog.blog_id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    TempData["SuccessMessage"] = "Đã cập nhật bài viết thành công!";
                    return RedirectToAction(nameof(ManageBlogs));
                }
                return View(blog);
            }


            // Hàm helper để tạo slug
            private string GenerateSlug(string title)
            {
                if (string.IsNullOrEmpty(title)) return "";

                // Chuyển hết sang chữ thường
                title = title.ToLowerInvariant();

                // Bỏ dấu
                var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(title);
                title = Encoding.ASCII.GetString(bytes);

                // Xóa các ký tự đặc biệt
                title = Regex.Replace(title, @"[^a-z0-9\s-]", "");

                // Thay thế khoảng trắng bằng dấu gạch ngang
                title = Regex.Replace(title, @"\s+", "-").Trim();

                // Cắt bớt nếu quá dài và đảm bảo không kết thúc bằng dấu gạch ngang
                return title.Substring(0, title.Length <= 100 ? title.Length : 100).TrimEnd('-');
            }
        }
    }
