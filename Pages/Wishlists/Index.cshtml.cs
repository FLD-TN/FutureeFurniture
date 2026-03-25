using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MTKPM_FE.Pages.Wishlists
{
    public class IndexModel : PageModel
    {
        private readonly myContext _db;
        public IList<WishList> WishLists { get; set; } = new List<WishList>();

        [BindProperty]
        public string NewName { get; set; }

        public IndexModel(myContext db) => _db = db;

        public async Task OnGetAsync()
        {
            var custJson = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(custJson)) return;

            int customerId = int.Parse(custJson);
            WishLists = await _db.WishLists
                .Where(w => w.CustomerId == customerId)
                .Include(w => w.Items)              // <-- d�ng Items
                    .ThenInclude(i => i.Product)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var custJson = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(custJson) || string.IsNullOrWhiteSpace(NewName))
                return RedirectToPage();

            int customerId = int.Parse(custJson);
            _db.WishLists.Add(new WishList
            {
                CustomerId = customerId,
                Name = NewName,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int itemId)
        {
            var it = await _db.WishListItems.FindAsync(itemId);
            if (it != null)
            {
                _db.WishListItems.Remove(it);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }

}
