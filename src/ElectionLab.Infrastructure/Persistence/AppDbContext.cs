using ElectionLab.Domain;
using Microsoft.EntityFrameworkCore;

namespace ElectionLab.Infrastructure.Persistence;

public class AppDbContext : DbContext 
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    
    public DbSet<Election> Elections => Set<Election>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}