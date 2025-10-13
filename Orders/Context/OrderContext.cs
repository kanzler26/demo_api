using Microsoft.EntityFrameworkCore;

namespace Orders.Context;

public class OrderContext(DbContextOptions<OrderContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}

public class Order
{
    public int Id { get; set; }
    public int OrderNumber { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
}