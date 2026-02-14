using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DICIS.Core.Entities;

public class UserProfile
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LocalGovernment { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    [StringLength(10)]
    public string Gender { get; set; } = string.Empty; // Male, Female, Other
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(50)]
    public string? City { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(11)]
    public string? NIN { get; set; } // National Identification Number
    
    [StringLength(500)]
    public string? ProfilePicture { get; set; } // Path to profile picture
    
    public bool IsEmailVerified { get; set; } = false;
    
    public DateTime? EmailVerifiedAt { get; set; }
    
    [StringLength(100)]
    public string? EmailVerificationToken { get; set; }
    
    public DateTime? EmailVerificationTokenExpires { get; set; }
    
    public bool IsProfileComplete { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
