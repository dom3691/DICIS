namespace DICIS.Core.DTOs;

public class RevokeCertificateRequest
{
    public string CertificateId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class FraudReportRequest
{
    public string? CertificateId { get; set; }
    public string? ReportedNIN { get; set; }
    public string ReportDescription { get; set; } = string.Empty;
    public string? ReporterName { get; set; }
    public string? ReporterEmail { get; set; }
    public string? ReporterPhone { get; set; }
}

public class AnalyticsResponse
{
    public int TotalApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ExceptionReviewApplications { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal AverageVerificationTimeSeconds { get; set; }
    public decimal AverageCertificateGenerationTimeSeconds { get; set; }
    public int FraudReportsCount { get; set; }
    public int RevokedCertificatesCount { get; set; }
    public Dictionary<string, int> ApplicationsByState { get; set; } = new();
    public Dictionary<string, int> ApplicationsByDay { get; set; } = new();
    public SLAMetrics SLA { get; set; } = new();
    public List<string> Alerts { get; set; } = new();
}

public class SLAMetrics
{
    public bool VerificationTimeExceeded { get; set; }
    public bool CertificateGenerationTimeExceeded { get; set; }
    public bool ApprovalRateBelowThreshold { get; set; }
    public int ApplicationsExceedingVerificationSLA { get; set; }
    public int ApplicationsExceedingCertificateGenerationSLA { get; set; }
    public decimal SLAComplianceRate { get; set; }
}
