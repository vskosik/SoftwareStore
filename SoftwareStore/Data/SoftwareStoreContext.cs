using Microsoft.EntityFrameworkCore;
using SoftwareStore.Models;

namespace SoftwareStore.Data
{
    public class SoftwareStoreContext : DbContext
    {
        public SoftwareStoreContext(DbContextOptions<SoftwareStoreContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<History> Histories { get; set; }
    }
}