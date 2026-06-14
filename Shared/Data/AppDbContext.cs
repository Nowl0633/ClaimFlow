using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // just add a new DbSet here whenever we add a new table
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
    }
}
