using System.ComponentModel.DataAnnotations;
using DICIS.Core.Entities;

namespace DICIS.Core.DTOs;

public class CreateServiceRequestDTO
{
    [Required]
    public ServiceType ServiceType { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ServiceRequestDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ServiceType ServiceType { get; set; }
    public ServiceRequestStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentReference { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ApplicationDTO? Application { get; set; }
    public CertificateDTO? Certificate { get; set; }
}

public class PaymentRequest
{
    [Required]
    public int ServiceRequestId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty; // Card, Bank Transfer, etc.
    
    [StringLength(500)]
    public string? PaymentDetails { get; set; } // JSON for payment gateway
}

public class PaymentResponse
{
    public bool IsSuccessful { get; set; }
    public string? PaymentReference { get; set; }
    public string? Message { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public class LocationResponse
{
    public List<string> Countries { get; set; } = new();
    public List<string> States { get; set; } = new();
    public List<string> LocalGovernments { get; set; } = new();
}
