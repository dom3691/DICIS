using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _certificateService;
    private readonly DicisDbContext _context;

    public CertificateController(ICertificateService certificateService, DicisDbContext context)
    {
        _certificateService = certificateService;
        _context = context;
    }

    [HttpPost("generate")]
    [Authorize]
    public async Task<ActionResult<CertificateDTO>> GenerateCertificate([FromBody] int applicationId)
    {
        try
        {
            var certificate = await _certificateService.GenerateCertificateAsync(applicationId);
            return Ok(certificate);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CertificateDTO>> GetCertificate(string id)
    {
        try
        {
            var verifyResult = await _certificateService.VerifyCertificateAsync(id);
            if (!verifyResult.IsValid)
            {
                return NotFound(verifyResult);
            }
            return Ok(verifyResult);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("verify/{certificateId}")]
    [AllowAnonymous]
    public async Task<ActionResult<CertificateVerifyResponse>> VerifyCertificate(string certificateId)
    {
        var result = await _certificateService.VerifyCertificateAsync(certificateId);
        return Ok(result);
    }

    [HttpGet("download/{certificateId}")]
    [Authorize]
    public async Task<IActionResult> DownloadCertificate(string certificateId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var certificate = await _context.Certificates
            .Include(c => c.Application)
            .ThenInclude(a => a.ServiceRequest)
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId);

        if (certificate == null)
        {
            return NotFound(new { message = "Certificate not found" });
        }

        // Check if user owns this certificate or is SuperAdmin
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "SuperAdmin" && certificate.Application.UserId != userId.Value)
        {
            return Forbid();
        }

        // Check if payment was completed
        if (certificate.Application.ServiceRequest != null && 
            certificate.Application.ServiceRequest.PaymentStatus != Core.Entities.PaymentStatus.Completed)
        {
            return BadRequest(new { message = "Payment is required before downloading certificate" });
        }

        if (certificate.Status != Core.Entities.CertificateStatus.Active)
        {
            return BadRequest(new { message = "Certificate is not active" });
        }

        try
        {
            var pdfBytes = await _certificateService.GetCertificatePDFAsync(certificateId);
            return File(pdfBytes, "application/pdf", $"{certificateId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    [HttpPost("revoke")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RevokeCertificate([FromBody] RevokeCertificateRequest request)
    {
        // Get admin user ID from claims
        var adminUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var result = await _certificateService.RevokeCertificateAsync(request.CertificateId, request.Reason, adminUserId);
        
        if (result)
        {
            return Ok(new { message = "Certificate revoked successfully" });
        }
        
        return NotFound(new { message = "Certificate not found" });
    }
}
