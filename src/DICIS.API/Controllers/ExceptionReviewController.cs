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
public class ExceptionReviewController : ControllerBase
{
    private readonly DicisDbContext _context;
    private readonly ICertificateService _certificateService;
    private readonly IAuditService _auditService;

    public ExceptionReviewController(
        DicisDbContext context,
        ICertificateService certificateService,
        IAuditService auditService)
    {
        _context = context;
        _certificateService = certificateService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationDTO>>> GetExceptionApplications()
    {
        var applications = await _context.Applications
            .Include(a => a.User)
            .Include(a => a.Certificate)
            .Where(a => a.Status == ApplicationStatus.ExceptionReview)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        var dtos = applications.Select(a => new ApplicationDTO
        {
            Id = a.Id,
            State = a.State,
            LGA = a.LGA,
            FatherNIN = a.FatherNIN,
            MotherNIN = a.MotherNIN,
            Status = a.Status,
            RiskScore = a.RiskScore,
            ConfidenceScore = a.ConfidenceScore,
            RejectionReason = a.RejectionReason,
            CreatedAt = a.CreatedAt,
            SubmittedAt = a.SubmittedAt,
            ApprovedAt = a.ApprovedAt,
            Certificate = a.Certificate != null ? new CertificateDTO
            {
                CertificateId = a.Certificate.CertificateId,
                QRCodeData = a.Certificate.QRCodeData,
                PDFPath = a.Certificate.PDFPath,
                Status = a.Certificate.Status,
                IssuedAt = a.Certificate.IssuedAt,
                ExpiresAt = a.Certificate.ExpiresAt
            } : null
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> ApproveApplication(int id, [FromBody] string? notes)
    {
        var application = await _context.Applications
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        if (application.Status != ApplicationStatus.ExceptionReview)
        {
            return BadRequest(new { message = "Application is not in exception review status" });
        }

        var adminUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        application.Status = ApplicationStatus.Approved;
        application.ApprovedAt = DateTime.UtcNow;
        application.VerifiedAt = DateTime.UtcNow;
        application.ReviewedBy = adminUserId;
        application.VerificationNotes = notes;

        await _context.SaveChangesAsync();

        // Generate certificate
        await _certificateService.GenerateCertificateAsync(id);

        await _auditService.LogActionAsync(
            "ApplicationApproved",
            "Application",
            id,
            null,
            $"Application approved by admin. Notes: {notes}",
            adminUserId,
            "Admin"
        );

        return Ok(new { message = "Application approved and certificate generated" });
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectApplication(int id, [FromBody] RejectApplicationRequest request)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        if (application.Status != ApplicationStatus.ExceptionReview)
        {
            return BadRequest(new { message = "Application is not in exception review status" });
        }

        var adminUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        application.Status = ApplicationStatus.Rejected;
        application.RejectionReason = request.Reason;
        application.ReviewedBy = adminUserId;
        application.VerificationNotes = request.Notes;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            "ApplicationRejected",
            "Application",
            id,
            null,
            $"Application rejected by admin. Reason: {request.Reason}",
            adminUserId,
            "Admin"
        );

        return Ok(new { message = "Application rejected" });
    }
}

public class RejectApplicationRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
