using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTKPM_FE.Helpers;


public class BlogController : Controller
{
    private readonly myContext _context;

    public BlogController(myContext context)
    {
        _context = context;
    }

    [Route("blog")]
    public async Task<IActionResult> Index(int? page)
    {
        var blogsQuery = _context.tbl_blog.AsNoTracking().OrderByDescending(b => b.CreatedAt);
        int pageSize = 6;
        int pageNumber = (page ?? 1);

        var paginatedBlogs = await PaginatedList<Blog>.CreateAsync(blogsQuery, pageNumber, pageSize);
        return View("News", paginatedBlogs);
    }

    [Route("blog/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return BadRequest();

        var blog = await _context.tbl_blog.FirstOrDefaultAsync(b => b.slug == slug);

        if (blog == null)
            return NotFound();

        return View("BlogDetail", blog);
    }
}
