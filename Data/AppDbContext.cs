using ClaimFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
    }
}