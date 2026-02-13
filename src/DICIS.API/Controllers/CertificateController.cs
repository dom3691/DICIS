using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificateController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
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
