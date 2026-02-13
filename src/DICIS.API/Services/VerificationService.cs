using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DICIS.API.Services;

public class VerificationService : IVerificationService
{
    private readonly DicisDbContext _context;
    private const decimal AUTO_APPROVE_THRESHOLD = 80m;
    private const decimal RISK_THRESHOLD = 30m;

    public VerificationService(DicisDbContext context)
    {
        _context = context;
    }

    public async Task<VerificationResponse> VerifyApplicationAsync(int applicationId)
    {
        var application = await _context.Applications
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
        
        if (application == null)
        {
            return new VerificationResponse
            {
                IsVerified = false,
                Status = "NotFound",
                Issues = new List<string> { "Application not found" }
            };
        }
        
        var issues = new List<string>();
        
        // Check for duplicate certificate
        var hasDuplicate = await CheckDuplicateCertificateAsync(application.User.NIN, application.State);
        if (hasDuplicate)
        {
            issues.Add("Duplicate certificate exists for this state");
        }
        
        // Verify parentage
        var parentageValid = await VerifyParentageAsync(
            application.User.NIN,
            application.FatherNIN,
            application.MotherNIN,
            application.State
        );
        
        if (!parentageValid && (!string.IsNullOrEmpty(application.FatherNIN) || !string.IsNullOrEmpty(application.MotherNIN)))
        {
            issues.Add("Parentage verification failed");
        }
        
        // Calculate scores
        var riskScore = await CalculateRiskScoreAsync(applicationId);
        var confidenceScore = await CalculateConfidenceScoreAsync(applicationId);
        
        application.RiskScore = riskScore;
        application.ConfidenceScore = confidenceScore;
        
        var requiresManualReview = riskScore > RISK_THRESHOLD || confidenceScore < AUTO_APPROVE_THRESHOLD || issues.Any();
        
        if (!requiresManualReview && !hasDuplicate)
        {
            application.Status = ApplicationStatus.Approved;
            application.ApprovedAt = DateTime.UtcNow;
            application.VerifiedAt = DateTime.UtcNow;
        }
        else if (hasDuplicate)
        {
            application.Status = ApplicationStatus.Rejected;
            application.RejectionReason = "Duplicate certificate exists";
        }
        else
        {
            application.Status = ApplicationStatus.ExceptionReview;
        }
        
        await _context.SaveChangesAsync();
        
        return new VerificationResponse
        {
            IsVerified = application.Status == ApplicationStatus.Approved,
            RiskScore = riskScore,
            ConfidenceScore = confidenceScore,
            Status = application.Status.ToString(),
            Issues = issues,
            RequiresManualReview = requiresManualReview
        };
    }

    public async Task<bool> VerifyParentageAsync(string applicantNIN, string? fatherNIN, string? motherNIN, string state)
    {
        // In production, this would call NIMC API to verify parent-child linkage
        // For now, simulate verification
        
        if (string.IsNullOrEmpty(fatherNIN) && string.IsNullOrEmpty(motherNIN))
        {
            return true; // No parent data provided
        }
        
        // Mock: If parent NINs are provided and match format, assume valid
        // In production, verify against NIMC database
        bool fatherValid = string.IsNullOrEmpty(fatherNIN) || 
                          (fatherNIN.Length == 11 && fatherNIN.All(char.IsDigit));
        bool motherValid = string.IsNullOrEmpty(motherNIN) || 
                          (motherNIN.Length == 11 && motherNIN.All(char.IsDigit));
        
        return fatherValid && motherValid;
    }

    public async Task<bool> CheckDuplicateCertificateAsync(string nin, string state)
    {
        return await _context.Applications
            .AnyAsync(a => a.User.NIN == nin && 
                          a.State == state && 
                          (a.Status == ApplicationStatus.Approved || 
                           a.Certificate != null && a.Certificate.Status == CertificateStatus.Active));
    }

    public async Task<decimal> CalculateRiskScoreAsync(int applicationId)
    {
        var application = await _context.Applications
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
        
        if (application == null) return 100m;
        
        decimal riskScore = 0m;
        
        // Check parentage mismatch
        if (!string.IsNullOrEmpty(application.FatherNIN) || !string.IsNullOrEmpty(application.MotherNIN))
        {
            var parentageValid = await VerifyParentageAsync(
                application.User.NIN,
                application.FatherNIN,
                application.MotherNIN,
                application.State
            );
            if (!parentageValid)
            {
                riskScore += 40m;
            }
        }
        
        // Check for missing supporting documents
        if (string.IsNullOrEmpty(application.SupportingDocuments))
        {
            riskScore += 10m;
        }
        
        // Check declaration
        if (!application.DeclarationAccepted)
        {
            riskScore += 50m;
        }
        
        // Check if user has multiple applications
        var applicationCount = await _context.Applications
            .CountAsync(a => a.UserId == application.UserId && a.Id != applicationId);
        if (applicationCount > 0)
        {
            riskScore += 20m;
        }
        
        return Math.Min(riskScore, 100m);
    }

    public async Task<decimal> CalculateConfidenceScoreAsync(int applicationId)
    {
        var application = await _context.Applications
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
        
        if (application == null) return 0m;
        
        decimal confidenceScore = 100m;
        
        // Reduce confidence based on risk factors
        var riskScore = await CalculateRiskScoreAsync(applicationId);
        confidenceScore -= riskScore;
        
        // Increase confidence if parentage verified
        if (!string.IsNullOrEmpty(application.FatherNIN) || !string.IsNullOrEmpty(application.MotherNIN))
        {
            var parentageValid = await VerifyParentageAsync(
                application.User.NIN,
                application.FatherNIN,
                application.MotherNIN,
                application.State
            );
            if (parentageValid)
            {
                confidenceScore += 20m;
            }
        }
        
        // Increase confidence if supporting documents provided
        if (!string.IsNullOrEmpty(application.SupportingDocuments))
        {
            confidenceScore += 10m;
        }
        
        return Math.Max(0m, Math.Min(confidenceScore, 100m));
    }
}
