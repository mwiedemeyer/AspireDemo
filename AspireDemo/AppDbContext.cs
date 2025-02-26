using Microsoft.EntityFrameworkCore;

namespace AspireDemo;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
           .HasPartitionKey(c => c.Id)
           .HasNoDiscriminator()
           .ToContainer("users");
    }
}

public class User
{
    public required string Id { get; set; }  
    public required string Name { get; set; }
}