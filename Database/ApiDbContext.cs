using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Database.Misc;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<AType> Types { get; set; }
    public DbSet<Point> Points { get; set; }
    public DbSet<AnimalVisitedPoint> AnimalVisitedPoints { get; set; }
    public DbSet<AnimalType> AnimalTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseIdentityColumns();
    }
}