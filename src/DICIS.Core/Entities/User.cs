using System.ComponentModel.DataAnnotations;

namespace DICIS.Core.Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(11)]
    public string NIN { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? MiddleName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public DateTime DateOfBirth { get; set; }
    
    [StringLength(10)]
    public string Gender { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
