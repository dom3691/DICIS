using Microsoft.EntityFrameworkCore;
using DICIS.Core.Entities;

namespace DICIS.Core.Data;

public class DicisDbContext : DbContext
{
    public DicisDbContext(DbContextOptions<DicisDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<FraudReport> FraudReports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.NIN)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // Configure ApplicationStatus enum to be stored as string
        modelBuilder.Entity<Application>()
            .Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // Application constraints - unique index for approved applications only
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.UserId, a.State })
            .IsUnique()
            .HasFilter("[Status] = 'Approved'");
        
        // Configure CertificateStatus enum to be stored as string
        modelBuilder.Entity<Certificate>()
            .Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // Certificate constraints
        modelBuilder.Entity<Certificate>()
            .HasIndex(c => c.CertificateId)
            .IsUnique();
        
        // Configure FraudReportStatus enum to be stored as string
        modelBuilder.Entity<FraudReport>()
            .Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // AdminUser constraints
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username)
            .IsUnique();
        
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();
    }
}
