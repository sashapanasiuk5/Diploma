using DBGuard.DataAccess.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DBGuard.DataAccess.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Alert> Alerts { get; set; }
    
    public DbSet<Rule> Rules { get; set; }
    
    public DbSet<Preference> Preferences { get; set; }
    
    public DbSet<DetectionCheckpoint> DetectionCheckpoints { get; set; }

    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DetectionCheckpoint>()
            .HasIndex(c => new { c.Type, c.EntityValue })
            .IsUnique();
        
        modelBuilder.Entity<Rule>()
            .Property(e => e.Key)
            .ValueGeneratedNever();
        
        base.OnModelCreating(modelBuilder);
    }
    
    // public AppDbContext()
    // {
       
    // }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //   optionsBuilder.UseSqlServer(
    //       "Server=localhost,1433;Database=DBGuard;User Id=webadmin;Password=123456;Encrypt=True;TrustServerCertificate=True");
    // }
}