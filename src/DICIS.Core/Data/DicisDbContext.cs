using Microsoft.EntityFrameworkCore;
using DICIS.Core.Entities;

namespace DICIS.Core.Data;

public class DicisDbContext : DbContext
{
    public DicisDbContext(DbContextOptions<DicisDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
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
        
        // Configure decimal properties with precision and scale
        modelBuilder.Entity<Application>()
            .Property(a => a.ConfidenceScore)
            .HasPrecision(5, 2); // e.g., 99.99
        
        modelBuilder.Entity<Application>()
            .Property(a => a.RiskScore)
            .HasPrecision(5, 2); // e.g., 99.99
        
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
        
        // Configure QRCodeData to allow larger base64 strings
        modelBuilder.Entity<Certificate>()
            .Property(c => c.QRCodeData)
            .HasMaxLength(5000);
        
        // Certificate constraints
        modelBuilder.Entity<Certificate>()
            .HasIndex(c => c.CertificateId)
            .IsUnique();
        
        // Configure FraudReportStatus enum to be stored as string
        modelBuilder.Entity<FraudReport>()
            .Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // Configure Role enum to be stored as string
        modelBuilder.Entity<AdminUser>()
            .Property(a => a.Role)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // AdminUser constraints
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username)
            .IsUnique();
        
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();
        
        // UserProfile constraints
        modelBuilder.Entity<UserProfile>()
            .HasIndex(u => u.UserId)
            .IsUnique();
        
        modelBuilder.Entity<UserProfile>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<UserProfile>()
            .HasIndex(u => u.EmailVerificationToken)
            .IsUnique()
            .HasFilter("[EmailVerificationToken] IS NOT NULL");
        
        // Configure ServiceType enum to be stored as string
        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.ServiceType)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // Configure ServiceRequestStatus enum to be stored as string
        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // Configure PaymentStatus enum to be stored as string
        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // ServiceRequest constraints
        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(s => s.PaymentReference)
            .IsUnique()
            .HasFilter("[PaymentReference] IS NOT NULL");
        
        // Application relationship with ServiceRequest
        modelBuilder.Entity<Application>()
            .HasOne(a => a.ServiceRequest)
            .WithOne(s => s.Application)
            .HasForeignKey<Application>(a => a.ServiceRequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
