using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MTKPM_FE.Pages.Wishlists
{
    public class DetailsModel : PageModel
    {
        private readonly myContext _db;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        // Non-nullable, kh?i t?o t?m b?ng default! ?? g�n trong OnGetAsync
        public WishList WishList { get; set; } = default!;
        public IList<Product> AvailableProducts { get; set; } = default!;

        public DetailsModel(myContext db) => _db = db;

        public async Task<IActionResult> OnGetAsync()
        {
            // Load wishlist v� items
            WishList = await _db.WishLists
                .Include(w => w.Items)
                  .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(w => w.WishListId == Id)
                ?? throw new InvalidOperationException("Wishlist not found");

            // Load t?t c? s?n ph?m ?? ch?n th�m
            AvailableProducts = await _db.tbl_product.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int productId)
        {
            // Th�m item v�o wishlist
            _db.WishListItems.Add(new WishListItem
            {
                WishListId = Id,
                ProductId = productId,
                AddedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return RedirectToPage(new { id = Id });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int itemId)
        {
            var it = await _db.WishListItems.FindAsync(itemId);
            if (it != null)
            {
                _db.WishListItems.Remove(it);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { id = Id });
        }
    }
}
