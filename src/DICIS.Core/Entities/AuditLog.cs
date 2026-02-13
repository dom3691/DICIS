using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DICIS.Core.Entities;

public class AuditLog
{
    [Key]
    public int Id { get; set; }
    
    public int? ApplicationId { get; set; }
    
    public int? CertificateId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // e.g., "ApplicationSubmitted", "CertificateGenerated", "CertificateRevoked"
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(1000)]
    public string? OldValues { get; set; } // JSON
    
    [StringLength(1000)]
    public string? NewValues { get; set; } // JSON
    
    public int? UserId { get; set; }
    
    [StringLength(50)]
    public string? UserRole { get; set; }
    
    [StringLength(50)]
    public string? IPAddress { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("ApplicationId")]
    public virtual Application? Application { get; set; }
}
