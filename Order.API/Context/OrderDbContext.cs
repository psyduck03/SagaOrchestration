using Microsoft.EntityFrameworkCore;

namespace Order.API.Context;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Models.Order> Orders { get; set; }
    public DbSet<Models.OrderItem> OrderItems { get; set; }
}