namespace DICIS.Core.DTOs;

public class VerificationRequest
{
    public int ApplicationId { get; set; }
}

public class VerificationResponse
{
    public bool IsVerified { get; set; }
    public decimal RiskScore { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
    public bool RequiresManualReview { get; set; }
}

public class CertificateVerifyRequest
{
    public string CertificateId { get; set; } = string.Empty;
}

public class CertificateVerifyResponse
{
    public bool IsValid { get; set; }
    public string? Name { get; set; }
    public string? State { get; set; }
    public string? LGA { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? IssuedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevocationReason { get; set; }
    public string? Message { get; set; }
}
