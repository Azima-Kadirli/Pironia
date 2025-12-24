using Microsoft.EntityFrameworkCore;
using Pronia.Models;

namespace Pronia.Context
{
    public class AppDbContext : DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer();
        //    base.OnConfiguring(optionsBuilder);
        //}

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
