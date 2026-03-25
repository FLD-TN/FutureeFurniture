using MTKPM_FE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

[AllowAnonymous]
public class CartController : Controller
{
    private readonly myContext _context;
    private readonly ILogger<CartController> _logger;
    public const string CARTKEY = "cart";

    public CartController(myContext context, ILogger<CartController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private List<CartItemViewModel> GetCartItems()
    {
        var cart = HttpContext.Session.Get<List<CartItemViewModel>>(CARTKEY);
        if (cart == null)
        {
            cart = new List<CartItemViewModel>();
        }
        return cart;
    }
    private void SaveCartSession(List<CartItemViewModel> cart)
    {
        HttpContext.Session.Set(CARTKEY, cart);
    }


    public IActionResult Index()
    {
        _logger.LogInformation("--- Đang vào action Index (trang giỏ hàng) ---");
        var cart = GetCartItems();
        _logger.LogInformation($"Số lượng sản phẩm trong giỏ hàng khi vào trang Index: {cart.Count}");
        return View(cart);
    }


    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult AddToCart(int productId, int quantity = 1)
    {
        _logger.LogInformation("--- Bắt đầu action AddToCart ---");
        _logger.LogInformation($"Nhận được yêu cầu thêm sản phẩm với ID: {productId}");

        try
        {
            var cart = GetCartItems();
            var cartItem = cart.Find(p => p.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _logger.LogInformation($"Sản phẩm ID {productId} đã có, tăng số lượng lên {cartItem.Quantity}");
            }
            else
            {
                _logger.LogInformation($"Sản phẩm ID {productId} chưa có, bắt đầu tìm trong DB...");
                var product = _context.tbl_product.FirstOrDefault(p => p.product_id == productId);

                if (product != null)
                {
                    cartItem = new CartItemViewModel
                    {
                        ProductId = product.product_id,
                        ProductName = product.product_name,
                        ProductImage = product.product_image,
                        Price = product.product_discount_price ?? product.product_price,
                        Quantity = quantity
                    };
                    cart.Add(cartItem);
                    _logger.LogInformation($"Đã tìm thấy sản phẩm '{product.product_name}' và thêm vào giỏ.");
                }
                else
                {
                    _logger.LogWarning($"!!! KHÔNG TÌM THẤY sản phẩm với ID: {productId} trong database.");
                }
            }

            _logger.LogInformation($"Tổng số sản phẩm trong giỏ hàng TRƯỚC KHI LƯU: {cart.Count}");
            SaveCartSession(cart);
            _logger.LogInformation("Đã gọi SaveCartSession.");

            // TRẢ LẠI DÒNG NÀY ĐỂ CÓ LUỒNG HOẠT ĐỘNG ĐÚNG
            return RedirectToAction("Index", "Cart");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!!!!!! LỖI NGHIÊM TRỌNG TRONG AddToCart !!!!!!");
            return StatusCode(500, "Đã có lỗi xảy ra trong quá trình xử lý. Vui lòng kiểm tra log.");
        }
    }


    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCartItems();
        var cartItem = cart.Find(p => p.ProductId == productId);

        if (cartItem != null)
        {
            cart.Remove(cartItem);
            SaveCartSession(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpdateCart(int productId, int quantity)
    {
        var cart = GetCartItems();
        var cartItem = cart.Find(p => p.ProductId == productId);

        if (cartItem != null)
        {
            if (quantity > 0)
            {
                cartItem.Quantity = quantity;
            }
            else
            {
                cart.Remove(cartItem);
            }
            SaveCartSession(cart);
        }

        return RedirectToAction("Index");
    }
}
