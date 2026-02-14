using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DICIS.Core.Entities;

public class Certificate
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ApplicationId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string CertificateId { get; set; } = string.Empty; // Unique certificate ID
    
    [Required]
    [StringLength(5000)]
    public string QRCodeData { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string PDFPath { get; set; } = string.Empty;
    
    [Required]
    [StringLength(64)]
    public string Hash { get; set; } = string.Empty; // SHA256 hash for integrity
    
    [StringLength(50)]
    public CertificateStatus Status { get; set; } = CertificateStatus.Active;
    
    [StringLength(500)]
    public string? RevocationReason { get; set; }
    
    public DateTime? RevokedAt { get; set; }
    
    public int? RevokedBy { get; set; } // Admin user ID
    
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddYears(10);
    
    // Navigation properties
    [ForeignKey("ApplicationId")]
    public virtual Application Application { get; set; } = null!;
}

public enum CertificateStatus
{
    Active,
    Revoked,
    Expired
}
