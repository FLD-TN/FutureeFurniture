using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTKPM_FE.Models
{
    public class myContext : DbContext
    {
        public myContext(DbContextOptions<myContext> options) : base(options)
        {
        }

        public DbSet<Admin> tbl_admin { get; set; }
        public DbSet<Customer> tbl_customer { get; set; }
        public DbSet<Category> tbl_category { get; set; }
        public DbSet<Product> tbl_product { get; set; }
        public DbSet<Cart> tbl_cart { get; set; }
        public DbSet<Feedback> tbl_feedback { get; set; }
        public DbSet<Faqs> tbl_faqs { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }
        public DbSet<Blog> tbl_blog { get; set; }
        public DbSet<Order> tbl_order { get; set; } = null!;
        public DbSet<OrderDetail> tbl_orderdetail { get; set; } = null!;
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<WishListItem> WishListItems { get; set; }
        public DbSet<ContactMessage> tbl_contact_message { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- QUAN HỆ KHÓA NGOẠI ---
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Product)
                .HasForeignKey(p => p.cat_id);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasPrecision(18, 2);

            // --- SEED ADMIN MẶC ĐỊNH ---
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                admin_id = 1,
                admin_name = "Administrator",
                admin_email = "admin@gmail.com",
                admin_password = "admin123",
                admin_image = "default-admin.png"
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
