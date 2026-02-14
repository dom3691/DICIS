using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
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
        
        // Generate and send OTP if NIN is valid
        if (response.IsValid && !string.IsNullOrEmpty(response.PhoneNumber))
        {
            var otp = await _authService.GenerateOTPAsync(request.NIN, response.PhoneNumber, response.Email ?? "");
            // In development, include OTP in response for testing
            // In production, OTP would be sent via SMS/Email only
            response.Message = $"NIN verified. OTP sent. (Dev: {otp})";
        }
        
        return Ok(response);
    }

    [HttpPost("otp-verify")]
    public async Task<ActionResult<OTPVerifyResponse>> VerifyOTP([FromBody] OTPVerifyRequest request)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, new OTPVerifyResponse
            {
                IsValid = false,
                Message = $"An error occurred: {ex.Message}"
            });
        }
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<AdminLoginResponse>> AdminLogin([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new AdminLoginResponse
            {
                IsValid = false,
                Message = "Username and password are required"
            });
        }

        var response = await _authService.AdminLoginAsync(request.Username, request.Password);

        if (!response.IsValid)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        await _authService.LogoutAsync(token);
        return Ok(new { message = "Logged out successfully" });
    }
}
