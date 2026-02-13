using DICIS.Core.Entities;

namespace DICIS.Core.DTOs;

public class ApplicationCreateRequest
{
    public string State { get; set; } = string.Empty;
    public string LGA { get; set; } = string.Empty;
    public string? FatherNIN { get; set; }
    public string? MotherNIN { get; set; }
    public List<string>? SupportingDocuments { get; set; }
    public bool DeclarationAccepted { get; set; }
}

public class ApplicationDTO
{
    public int Id { get; set; }
    public string State { get; set; } = string.Empty;
    public string LGA { get; set; } = string.Empty;
    public string? FatherNIN { get; set; }
    public string? MotherNIN { get; set; }
    public ApplicationStatus Status { get; set; }
    public decimal RiskScore { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public CertificateDTO? Certificate { get; set; }
}

public class CertificateDTO
{
    public string CertificateId { get; set; } = string.Empty;
    public string QRCodeData { get; set; } = string.Empty;
    public string PDFPath { get; set; } = string.Empty;
    public CertificateStatus Status { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
