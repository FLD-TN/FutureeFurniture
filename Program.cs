using MTKPM_FE.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// 2. Thêm Razor Pages
builder.Services.AddRazorPages();

// 2.1. Thêm Blazor Server với Circuit Options
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => {
        options.DetailedErrors = true;
    });

// 3. Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<myContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("myconnection")));

// 4. Đăng ký bộ nhớ đệm để hỗ trợ Session
builder.Services.AddDistributedMemoryCache();

// 5. Cấu hình Session
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(2);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

// 6. Đăng ký IHttpContextAccessor để có thể inject vào Controller
builder.Services.AddHttpContextAccessor();

// 6.1. Đăng ký HttpClient cho Blazor components
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddHttpClient<LogApiClient>();
var app = builder.Build();

// 7. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Xếp Session trước Authorization và trước MapEndpoints
app.UseSession();

app.UseAuthorization();

// 8. Map Razor Pages và MVC routes
app.MapRazorPages();

// 8.1. Map Blazor Hub
app.MapBlazorHub();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
