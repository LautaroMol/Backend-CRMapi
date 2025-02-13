using Microsoft.EntityFrameworkCore;

namespace CRMapi.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public DbSet<Entity.Product> Products { get; set; }
        public DbSet<Entity.Orders> Orders { get; set; }
        public DbSet<Entity.OrderDetails> OrderDetails { get; set; }
        public DbSet<Entity.Clients> Customers { get; set; }
        public DbSet<Entity.Personal> Personals { get; set; }
    }
}
