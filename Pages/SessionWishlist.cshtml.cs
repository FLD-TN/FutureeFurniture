using System.Collections.Generic;
using System.Linq;
using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace MTKPM_FE.Pages
{
    public class SessionWishlistModel : PageModel
    {
        private const string SESSION_KEY = "wishlist";
        private readonly myContext _db;

        public SessionWishlistModel(myContext db) => _db = db;

        public List<Product> Products { get; set; } = new();

        public void OnGet()
        {
            var json = HttpContext.Session.GetString(SESSION_KEY);
            if (string.IsNullOrEmpty(json)) return;
            var ids = JsonConvert.DeserializeObject<List<int>>(json)!;
            Products = _db.tbl_product
                          .Where(p => ids.Contains(p.product_id))
                          .ToList();
        }

        public IActionResult OnPostRemove(int id)
        {
            var json = HttpContext.Session.GetString(SESSION_KEY) ?? "[]";
            var ids = JsonConvert.DeserializeObject<List<int>>(json)!;

            if (ids.Remove(id))
                HttpContext.Session.SetString(SESSION_KEY, JsonConvert.SerializeObject(ids));

            return RedirectToPage();
        }

        public IActionResult OnPostClearAll()
        {
            HttpContext.Session.Remove(SESSION_KEY);
            return RedirectToPage();
        }
    }
}
