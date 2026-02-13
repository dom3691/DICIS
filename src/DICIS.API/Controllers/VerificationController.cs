using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;

    public VerificationController(IVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    [HttpPost("run")]
    public async Task<ActionResult<VerificationResponse>> RunVerification([FromBody] VerificationRequest request)
    {
        var result = await _verificationService.VerifyApplicationAsync(request.ApplicationId);
        return Ok(result);
    }

    [HttpGet("status/{applicationId}")]
    public async Task<ActionResult<VerificationResponse>> GetVerificationStatus(int applicationId)
    {
        var result = await _verificationService.VerifyApplicationAsync(applicationId);
        return Ok(result);
    }
}
