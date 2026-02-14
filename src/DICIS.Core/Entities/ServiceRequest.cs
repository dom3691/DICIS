using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DICIS.Core.Entities;

public class ServiceRequest
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public ServiceType ServiceType { get; set; }
    
    [Required]
    [StringLength(50)]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
    
    [Required]
    public decimal Amount { get; set; }
    
    [StringLength(100)]
    public string? PaymentReference { get; set; }
    
    [StringLength(50)]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    public DateTime? PaymentDate { get; set; }
    
    [StringLength(500)]
    public string? PaymentDetails { get; set; } // JSON for payment gateway response
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    public virtual Application? Application { get; set; }
    
    public virtual Certificate? Certificate { get; set; }
}

public enum ServiceType
{
    IndigeneCertificate,
    StateOfOrigin,
    LocalGovernmentCertificate,
    Other
}

public enum ServiceRequestStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    Rejected
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}
