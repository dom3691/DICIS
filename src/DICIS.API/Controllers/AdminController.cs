using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly DicisDbContext _context;

    public AdminController(DicisDbContext context)
    {
        _context = context;
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsResponse>> GetAnalytics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var applications = await _context.Applications
            .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
            .ToListAsync();

        var totalApplications = applications.Count;
        var approvedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved);
        var rejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected);
        var pendingApplications = applications.Count(a => a.Status == ApplicationStatus.PendingVerification);
        var exceptionReviewApplications = applications.Count(a => a.Status == ApplicationStatus.ExceptionReview);

        var approvalRate = totalApplications > 0 ? (decimal)approvedApplications / totalApplications * 100 : 0;

        // Calculate average verification time
        var verifiedApplications = applications.Where(a => a.VerifiedAt.HasValue && a.SubmittedAt.HasValue).ToList();
        var avgVerificationTime = verifiedApplications.Any() 
            ? verifiedApplications.Average(a => (a.VerifiedAt!.Value - a.SubmittedAt!.Value).TotalSeconds)
            : 0;

        // Calculate average certificate generation time
        var certificatesWithTiming = await _context.Certificates
            .Include(c => c.Application)
            .Where(c => c.Application.ApprovedAt.HasValue && c.IssuedAt >= from && c.IssuedAt <= to)
            .ToListAsync();
        
        var avgCertificateGenerationTime = certificatesWithTiming.Any()
            ? certificatesWithTiming.Average(c => (c.IssuedAt - c.Application.ApprovedAt!.Value).TotalSeconds)
            : 0;

        // SLA Monitoring (60 seconds for verification, 10 seconds for certificate generation)
        const double verificationSLASeconds = 60;
        const double certificateGenerationSLASeconds = 10;
        const decimal approvalRateThreshold = 80m;

        var applicationsExceedingVerificationSLA = verifiedApplications
            .Count(a => (a.VerifiedAt!.Value - a.SubmittedAt!.Value).TotalSeconds > verificationSLASeconds);

        var applicationsExceedingCertificateGenerationSLA = certificatesWithTiming
            .Count(c => (c.IssuedAt - c.Application.ApprovedAt!.Value).TotalSeconds > certificateGenerationSLASeconds);

        var slaComplianceRate = verifiedApplications.Any()
            ? (decimal)((verifiedApplications.Count - applicationsExceedingVerificationSLA) * 100.0 / verifiedApplications.Count)
            : 100m;

        var slaMetrics = new SLAMetrics
        {
            VerificationTimeExceeded = avgVerificationTime > verificationSLASeconds,
            CertificateGenerationTimeExceeded = avgCertificateGenerationTime > certificateGenerationSLASeconds,
            ApprovalRateBelowThreshold = approvalRate < approvalRateThreshold,
            ApplicationsExceedingVerificationSLA = applicationsExceedingVerificationSLA,
            ApplicationsExceedingCertificateGenerationSLA = applicationsExceedingCertificateGenerationSLA,
            SLAComplianceRate = slaComplianceRate
        };

        // Generate alerts
        var alerts = new List<string>();
        if (slaMetrics.VerificationTimeExceeded)
        {
            alerts.Add($"⚠️ WARNING: Average verification time ({avgVerificationTime:F2}s) exceeds SLA ({verificationSLASeconds}s)");
        }
        if (slaMetrics.CertificateGenerationTimeExceeded)
        {
            alerts.Add($"⚠️ WARNING: Average certificate generation time ({avgCertificateGenerationTime:F2}s) exceeds SLA ({certificateGenerationSLASeconds}s)");
        }
        if (slaMetrics.ApprovalRateBelowThreshold)
        {
            alerts.Add($"⚠️ WARNING: Approval rate ({approvalRate:F1}%) is below threshold ({approvalRateThreshold}%)");
        }
        if (applicationsExceedingVerificationSLA > 0)
        {
            alerts.Add($"⚠️ {applicationsExceedingVerificationSLA} application(s) exceeded verification SLA");
        }
        if (applicationsExceedingCertificateGenerationSLA > 0)
        {
            alerts.Add($"⚠️ {applicationsExceedingCertificateGenerationSLA} certificate(s) exceeded generation SLA");
        }

        var fraudReportsCount = await _context.FraudReports
            .CountAsync(f => f.CreatedAt >= from && f.CreatedAt <= to);

        var revokedCertificatesCount = await _context.Certificates
            .CountAsync(c => c.RevokedAt.HasValue && c.RevokedAt >= from && c.RevokedAt <= to);

        var applicationsByState = applications
            .GroupBy(a => a.State)
            .ToDictionary(g => g.Key, g => g.Count());

        var applicationsByDay = applications
            .GroupBy(a => a.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

        return Ok(new AnalyticsResponse
        {
            TotalApplications = totalApplications,
            ApprovedApplications = approvedApplications,
            RejectedApplications = rejectedApplications,
            PendingApplications = pendingApplications,
            ExceptionReviewApplications = exceptionReviewApplications,
            ApprovalRate = approvalRate,
            AverageVerificationTimeSeconds = (decimal)avgVerificationTime,
            AverageCertificateGenerationTimeSeconds = (decimal)avgCertificateGenerationTime,
            FraudReportsCount = fraudReportsCount,
            RevokedCertificatesCount = revokedCertificatesCount,
            ApplicationsByState = applicationsByState,
            ApplicationsByDay = applicationsByDay,
            SLA = slaMetrics,
            Alerts = alerts
        });
    }

    [HttpPost("fraud-report")]
    [AllowAnonymous]
    public async Task<ActionResult> SubmitFraudReport([FromBody] FraudReportRequest request)
    {
        var fraudReport = new FraudReport
        {
            CertificateId = request.CertificateId,
            ReportedNIN = request.ReportedNIN,
            ReportDescription = request.ReportDescription,
            ReporterName = request.ReporterName,
            ReporterEmail = request.ReporterEmail,
            ReporterPhone = request.ReporterPhone,
            Status = FraudReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.FraudReports.Add(fraudReport);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Fraud report submitted successfully", id = fraudReport.Id });
    }
}
