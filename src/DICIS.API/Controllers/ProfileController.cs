using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpPost]
    public async Task<ActionResult<ProfileDTO>> CreateProfile([FromBody] CreateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var profile = await _profileService.CreateProfileAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetProfile), new { }, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDTO>> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var profile = await _profileService.GetProfileAsync(userId.Value);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDTO>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var profile = await _profileService.UpdateProfileAsync(userId.Value, request);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _profileService.VerifyEmailAsync(request.Token);
        if (result)
        {
            return Ok(new { message = "Email verified successfully" });
        }
        return BadRequest(new { message = "Invalid or expired verification token" });
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
    {
        var result = await _profileService.ResendVerificationEmailAsync(request.Email);
        if (result)
        {
            return Ok(new { message = "Verification email sent" });
        }
        return BadRequest(new { message = "Email not found" });
    }

    [HttpGet("check-complete")]
    public async Task<ActionResult> CheckProfileComplete()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var isComplete = await _profileService.IsProfileCompleteAsync(userId.Value);
        var isVerified = await _profileService.IsEmailVerifiedAsync(userId.Value);

        return Ok(new
        {
            IsProfileComplete = isComplete,
            IsEmailVerified = isVerified
        });
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
}
