using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MTKPM_FE.ViewComponents
{
    // DTO cho item trong preview
    public record WishlistProductDto(int ProductId, string ProductName, string ProductImage);

    // ViewModel dùng trong ViewComponent
    public class WishlistSummaryViewModel
    {
        public int Count { get; set; }
        public List<WishlistProductDto> Preview { get; set; } = new();
    }

    public class WishlistSummaryViewComponent : ViewComponent
    {
        private readonly myContext _db;
        private const string SESSION_KEY = "wishlist";

        public WishlistSummaryViewComponent(myContext db) => _db = db;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy danh sách product_id từ session
            var json = HttpContext.Session.GetString(SESSION_KEY);
            var ids = string.IsNullOrEmpty(json)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(json)!;

            // Lấy preview (tối đa 3 sản phẩm)
            var preview = await _db.tbl_product
                .Where(p => ids.Contains(p.product_id))
                .Select(p => new WishlistProductDto(
                    p.product_id,
                    p.product_name,
                    p.product_image))
                .Take(3)
                .ToListAsync();

            // Khởi tạo ViewModel
            var vm = new WishlistSummaryViewModel
            {
                Count = ids.Count,
                Preview = preview
            };

            return View(vm.Count);
        }
    }
}
