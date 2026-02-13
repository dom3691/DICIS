using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DICIS.Core.Entities;

public class Application
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LGA { get; set; } = string.Empty;
    
    [StringLength(11)]
    public string? FatherNIN { get; set; }
    
    [StringLength(11)]
    public string? MotherNIN { get; set; }
    
    [StringLength(500)]
    public string? SupportingDocuments { get; set; } // JSON array of document paths
    
    public bool DeclarationAccepted { get; set; }
    
    [StringLength(50)]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    
    public decimal RiskScore { get; set; }
    
    public decimal ConfidenceScore { get; set; }
    
    [StringLength(500)]
    public string? RejectionReason { get; set; }
    
    [StringLength(500)]
    public string? VerificationNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? VerifiedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public int? ReviewedBy { get; set; } // Admin user ID
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    public virtual Certificate? Certificate { get; set; }
    
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public enum ApplicationStatus
{
    Draft,
    PendingVerification,
    Approved,
    Rejected,
    ExceptionReview,
    Revoked
}
