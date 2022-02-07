using Microsoft.EntityFrameworkCore;

namespace IdentitySample.WebApp.Data;

public class IdentityDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, UserName = "TestUser", Password = "Password" }
        );
    }
}