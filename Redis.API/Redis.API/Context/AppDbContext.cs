using Microsoft.EntityFrameworkCore;
using Redis.API.Models;

namespace Redis.API.Context
{
    public class AppDbContext : DbContext
    {
        // constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        // my tables
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product(){ Id = 1, Name = "Keyboard", Price = 200 },
                new Product() { Id = 2, Name = "Laptop", Price = 18000 },
                new Product() { Id = 3, Name = "Monitor", Price = 5000 },
                new Product() { Id = 4, Name = "Mouse", Price = 100 }
                );
        }
    }
}
