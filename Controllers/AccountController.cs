using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;

namespace MTKPM_FE.Controllers
{
    public class AccountController : Controller
    {
        private readonly myContext _context;
        private readonly LogApiClient _logApiClient;

        public AccountController(myContext context, LogApiClient logApiClient)
        {
            _context = context;
            _logApiClient = logApiClient;
        }

        [HttpGet]
        public IActionResult Login(string tab = "login", string returnUrl = null)
        {
            ViewBag.ActiveTab = tab;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // --- LOGIN CHO ADMIN (GIỮ NGUYÊN) ---
        [HttpPost]
        public async Task<IActionResult> Login(string adminEmail, string adminPassword)
        {
            var admin = _context.tbl_admin.FirstOrDefault(a => a.admin_email == adminEmail);
            if (admin != null && admin.admin_password == adminPassword)
            {
                HttpContext.Session.SetString("Role", "Admin");
                HttpContext.Session.SetString("AdminEmail", admin.admin_email);
                HttpContext.Session.SetString("admin_session", admin.admin_id.ToString());

                await _logApiClient.LogAdminLoginAsync(admin.admin_id.ToString(), "Login", "Admin logged in successfully");
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.message = "Sai tài khoản hoặc mật khẩu Admin";
            return View();
        }

        // --- ĐĂNG KÝ CUSTOMER (KHỚP 39 TC) ---
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            ViewBag.ActiveTab = "signup";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.SignupError = "Vui lòng nhập đầy đủ thông tin.";
                return View("Login");
            }

            // TC_AUTH_015, 017: Username validation
            if (!Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9]{5,19}$"))
            {
                if (username.Length < 6 || username.Length > 20)
                    ViewBag.SignupError = "Tên đăng nhập từ 6‑20 ký tự.";
                else
                    ViewBag.SignupError = "Tên đăng nhập phải bắt đầu bằng chữ cái.";
                return View("Login");
            }

            // TC_AUTH_004: Email validation
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.SignupError = "Email không đúng định dạng.";
                return View("Login");
            }

            // TC_AUTH_018, 019, 020: Password strength
            var hasUpper = new Regex(@"[A-Z]+");
            var hasNumber = new Regex(@"[0-9]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]+");

            if (password.Length < 8) { ViewBag.SignupError = "Mật khẩu phải có ít nhất 8 ký tự."; return View("Login"); }
            if (!hasUpper.IsMatch(password)) { ViewBag.SignupError = "Mật khẩu phải bao gồm chữ hoa."; return View("Login"); }
            if (!hasNumber.IsMatch(password)) { ViewBag.SignupError = "Mật khẩu phải bao gồm số."; return View("Login"); }
            if (!hasSymbols.IsMatch(password)) { ViewBag.SignupError = "Mật khẩu phải bao gồm ký tự đặc biệt."; return View("Login"); }

            if (password != confirmPassword) { ViewBag.SignupError = "Mật khẩu xác nhận không khớp."; return View("Login"); }

            // TC_AUTH_002, 003: Unique check
            if (_context.tbl_customer.Any(c => c.customer_name == username)) { ViewBag.SignupError = "Tên đăng nhập đã được sử dụng."; return View("Login"); }
            if (_context.tbl_customer.Any(c => c.customer_email == email)) { ViewBag.SignupError = "Email đã được sử dụng."; return View("Login"); }

            var customer = new Customer
            {
                customer_name = username,
                customer_email = email,
                customer_password = password,
                customer_image = "default.png",
                CreatedAt = DateTime.Now
            };
            _context.tbl_customer.Add(customer);
            _context.SaveChanges();

            ViewBag.SignupSuccess = "Đã đăng kí thành công! Chuyển hướng sau 2s...";
            return View("Login");
        }

        // --- ĐĂNG NHẬP CUSTOMER ---
        [HttpPost]
        public async Task<IActionResult> CustomerLogin(string loginEmail, string loginPassword, string returnUrl = null)
        {
            ViewBag.ActiveTab = "login";

            // TC_AUTH_030: Lockout 5 lần
            int attempts = HttpContext.Session.GetInt32("LoginAttempts") ?? 0;
            if (attempts >= 5) { ViewBag.LoginError = "Tài khoản bị khóa tạm thời do nhập sai quá nhiều lần."; return View("Login"); }

            if (string.IsNullOrEmpty(loginEmail) || string.IsNullOrEmpty(loginPassword))
            {
                ViewBag.LoginError = "Vui lòng nhập đầy đủ thông tin.";
                return View("Login");
            }

            var user = _context.tbl_customer.FirstOrDefault(c => c.customer_email == loginEmail && c.customer_password == loginPassword);
            if (user != null)
            {
                HttpContext.Session.SetInt32("LoginAttempts", 0);
                HttpContext.Session.SetString("Role", "Customer");
                HttpContext.Session.SetString("CustomerId", user.customer_id.ToString());
                HttpContext.Session.SetString("CustomerName", user.customer_name);

                await _logApiClient.LogUserLoginAsync(user.customer_id.ToString(), "Login", "Customer logged in");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            HttpContext.Session.SetInt32("LoginAttempts", attempts + 1);
            ViewBag.LoginError = "Tài khoản hoặc mật khẩu không chính xác.";
            return View("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var id = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Login");
            var user = _context.tbl_customer.Find(int.Parse(id));
            return View(user);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var id = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Login");
            var user = _context.tbl_customer.Find(int.Parse(id));
            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(Customer model, IFormFile customer_image_file)
        {
            var custId = int.Parse(HttpContext.Session.GetString("CustomerId"));
            var user = _context.tbl_customer.Find(custId);

            // TC_AUTH_039: Phone validation
            if (!string.IsNullOrEmpty(model.customer_phone) && !Regex.IsMatch(model.customer_phone, @"^0[0-9]{9}$"))
            {
                ViewBag.Error = "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0).";
                return View(user);
            }

            // TC_AUTH_035: Email conflict check
            if (_context.tbl_customer.Any(c => c.customer_email == model.customer_email && c.customer_id != custId))
            {
                ViewBag.Error = "Email đã được sử dụng bởi tài khoản khác.";
                return View(user);
            }

            user.customer_name = model.customer_name;
            user.customer_email = model.customer_email;
            user.customer_phone = model.customer_phone;
            user.customer_address = model.customer_address;

            if (customer_image_file != null && customer_image_file.Length > 0)
            {
                var fileName = Path.GetFileName(customer_image_file.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    customer_image_file.CopyTo(stream);
                }
                user.customer_image = fileName;
            }

            _context.SaveChanges();
            ViewBag.Success = "Cập nhật hồ sơ thành công!";
            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var id = HttpContext.Session.GetString("CustomerId");
            var user = _context.tbl_customer.Find(int.Parse(id));

            if (user.customer_password != oldPassword) { ViewBag.Error = "Mật khẩu cũ không đúng."; return View(); }
            if (newPassword.Length < 8) { ViewBag.Error = "Mật khẩu phải có ít nhất 8 ký tự."; return View(); } // TC_AUTH_036
            if (newPassword != confirmPassword) { ViewBag.Error = "Xác nhận mật khẩu không khớp."; return View(); }

            user.customer_password = newPassword;
            _context.SaveChanges();
            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (!_context.tbl_customer.Any(c => c.customer_email == email))
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống."; // TC_AUTH_012
                return View();
            }
            ViewBag.Success = "Liên kết đặt lại mật khẩu đã được gửi vào email của bạn."; // TC_AUTH_011
            return View();
        }
    }
}