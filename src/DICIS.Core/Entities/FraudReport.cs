using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DICIS.Core.Entities;

public class FraudReport
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(50)]
    public string? CertificateId { get; set; }
    
    [StringLength(11)]
    public string? ReportedNIN { get; set; }
    
    [Required]
    [StringLength(500)]
    public string ReportDescription { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ReporterName { get; set; }
    
    [StringLength(100)]
    public string? ReporterEmail { get; set; }
    
    [StringLength(20)]
    public string? ReporterPhone { get; set; }
    
    [StringLength(50)]
    public FraudReportStatus Status { get; set; } = FraudReportStatus.Pending;
    
    [StringLength(500)]
    public string? AdminNotes { get; set; }
    
    public int? ReviewedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReviewedAt { get; set; }
    
    [ForeignKey("ReviewedBy")]
    public virtual AdminUser? ReviewedByAdmin { get; set; }
}

public enum FraudReportStatus
{
    Pending,
    UnderInvestigation,
    Resolved,
    Dismissed
}
