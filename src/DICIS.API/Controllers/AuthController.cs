using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("nin-verify")]
    public async Task<ActionResult<NINVerifyResponse>> VerifyNIN([FromBody] NINVerifyRequest request)
    {
        if (string.IsNullOrEmpty(request.NIN))
        {
            return BadRequest(new NINVerifyResponse
            {
                IsValid = false,
                Message = "NIN is required"
            });
        }

        var response = await _authService.VerifyNINAsync(request.NIN);
        return Ok(response);
    }

    [HttpPost("otp-verify")]
    public async Task<ActionResult<OTPVerifyResponse>> VerifyOTP([FromBody] OTPVerifyRequest request)
    {
        if (string.IsNullOrEmpty(request.NIN) || string.IsNullOrEmpty(request.OTP))
        {
            return BadRequest(new OTPVerifyResponse
            {
                IsValid = false,
                Message = "NIN and OTP are required"
            });
        }

        var response = await _authService.VerifyOTPAsync(request.NIN, request.OTP);
        
        if (!response.IsValid)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }
}
