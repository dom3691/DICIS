using System.ComponentModel.DataAnnotations;

namespace DICIS.Core.Entities;

public class AdminUser
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Role { get; set; } = "Admin"; // Admin, StateAdmin, LGAAdmin
    
    [StringLength(50)]
    public string? State { get; set; } // For state-specific admins
    
    [StringLength(100)]
    public string? LGA { get; set; } // For LGA-specific admins
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
}
