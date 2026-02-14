using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly DicisDbContext _context;
    private readonly IVerificationService _verificationService;
    private readonly ICertificateService _certificateService;

    public ApplicationsController(
        DicisDbContext context,
        IVerificationService verificationService,
        ICertificateService certificateService)
    {
        _context = context;
        _verificationService = verificationService;
        _certificateService = certificateService;
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationDTO>> CreateApplication([FromBody] ApplicationCreateRequest request)
    {
        // Check if user is an admin - admins cannot create applications
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "SuperAdmin" || userRole == "Admin")
        {
            return BadRequest(new { message = "Administrators cannot create applications. Please login as a citizen to create an application." });
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Verify the user exists in the Users table (not AdminUsers)
        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return BadRequest(new { message = "User not found. Please login as a citizen to create an application." });
        }

        // Check for duplicate
        var existing = await _context.Applications
            .FirstOrDefaultAsync(a => a.UserId == userId && a.State == request.State && 
                                     (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.PendingVerification));
        
        if (existing != null)
        {
            return BadRequest(new { message = "Application already exists for this state" });
        }

        var application = new Application
        {
            UserId = userId.Value,
            State = request.State,
            LGA = request.LGA,
            FatherNIN = request.FatherNIN,
            MotherNIN = request.MotherNIN,
            SupportingDocuments = request.SupportingDocuments != null ? 
                System.Text.Json.JsonSerializer.Serialize(request.SupportingDocuments) : null,
            DeclarationAccepted = request.DeclarationAccepted,
            Status = ApplicationStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        var dto = MapToDTO(application);
        return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, dto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationDTO>> GetApplication(int id)
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var application = await _context.Applications
            .Include(a => a.Certificate)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        // SuperAdmin can see any application, regular users can only see their own
        if (userRole != "SuperAdmin")
        {
            var userId = GetUserId();
            if (userId == null || application.UserId != userId)
            {
                return Forbid();
            }
        }

        return Ok(MapToDTO(application));
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationDTO>>> GetApplications()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // SuperAdmin can see all applications, regular users see only their own
        if (userRole == "SuperAdmin")
        {
            var allApplications = await _context.Applications
                .Include(a => a.Certificate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return Ok(allApplications.Select(MapToDTO).ToList());
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var applications = await _context.Applications
            .Include(a => a.Certificate)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(applications.Select(MapToDTO).ToList());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApplicationDTO>> UpdateApplication(int id, [FromBody] ApplicationCreateRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (application == null)
        {
            return NotFound();
        }

        if (application.Status != ApplicationStatus.Draft)
        {
            return BadRequest(new { message = "Only draft applications can be updated" });
        }

        application.State = request.State;
        application.LGA = request.LGA;
        application.FatherNIN = request.FatherNIN;
        application.MotherNIN = request.MotherNIN;
        application.SupportingDocuments = request.SupportingDocuments != null ? 
            System.Text.Json.JsonSerializer.Serialize(request.SupportingDocuments) : null;
        application.DeclarationAccepted = request.DeclarationAccepted;

        await _context.SaveChangesAsync();

        return Ok(MapToDTO(application));
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ApplicationDTO>> SubmitApplication(int id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (application == null)
        {
            return NotFound();
        }

        if (application.Status != ApplicationStatus.Draft)
        {
            return BadRequest(new { message = "Application already submitted" });
        }

        if (!application.DeclarationAccepted)
        {
            return BadRequest(new { message = "Declaration must be accepted" });
        }

        application.Status = ApplicationStatus.PendingVerification;
        application.SubmittedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Trigger automated verification
        var verificationResult = await _verificationService.VerifyApplicationAsync(id);

        // If auto-approved, generate certificate
        if (verificationResult.IsVerified && !verificationResult.RequiresManualReview)
        {
            await _certificateService.GenerateCertificateAsync(id);
        }

        return Ok(MapToDTO(application));
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private ApplicationDTO MapToDTO(Application application)
    {
        return new ApplicationDTO
        {
            Id = application.Id,
            State = application.State,
            LGA = application.LGA,
            FatherNIN = application.FatherNIN,
            MotherNIN = application.MotherNIN,
            Status = application.Status,
            RiskScore = application.RiskScore,
            ConfidenceScore = application.ConfidenceScore,
            RejectionReason = application.RejectionReason,
            CreatedAt = application.CreatedAt,
            SubmittedAt = application.SubmittedAt,
            ApprovedAt = application.ApprovedAt,
            Certificate = application.Certificate != null ? new CertificateDTO
            {
                CertificateId = application.Certificate.CertificateId,
                QRCodeData = application.Certificate.QRCodeData,
                PDFPath = application.Certificate.PDFPath,
                Status = application.Certificate.Status,
                IssuedAt = application.Certificate.IssuedAt,
                ExpiresAt = application.Certificate.ExpiresAt
            } : null
        };
    }
}
