using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class WishlistController : Controller
{
    private const string SESSION_KEY = "wishlist";

    [HttpPost]
    public IActionResult AddToDefault(int productId)
    {
        // Lấy danh sách từ session
        var json = HttpContext.Session.GetString(SESSION_KEY);
        var list = string.IsNullOrEmpty(json)
            ? new List<int>()
            : JsonConvert.DeserializeObject<List<int>>(json)!;

        if (!list.Contains(productId))
            list.Add(productId);

        // Lưu lại
        HttpContext.Session.SetString(SESSION_KEY, JsonConvert.SerializeObject(list));
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
