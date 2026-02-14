using DICIS.Core.DTOs;
using DICIS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRequestController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IProfileService _profileService;

    public ServiceRequestController(
        IServiceRequestService serviceRequestService,
        IProfileService profileService)
    {
        _serviceRequestService = serviceRequestService;
        _profileService = profileService;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDTO>> CreateServiceRequest([FromBody] CreateServiceRequestDTO request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Check if profile is complete and email is verified
        var isProfileComplete = await _profileService.IsProfileCompleteAsync(userId.Value);
        var isEmailVerified = await _profileService.IsEmailVerifiedAsync(userId.Value);

        if (!isProfileComplete)
        {
            return BadRequest(new { message = "Please complete your profile first" });
        }

        if (!isEmailVerified)
        {
            return BadRequest(new { message = "Please verify your email address first" });
        }

        var serviceRequest = await _serviceRequestService.CreateServiceRequestAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.Id }, serviceRequest);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequestDTO>> GetServiceRequest(int id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var serviceRequest = await _serviceRequestService.GetServiceRequestAsync(id, userId.Value);
        if (serviceRequest == null)
        {
            return NotFound();
        }

        return Ok(serviceRequest);
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceRequestDTO>>> GetServiceRequests()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var serviceRequests = await _serviceRequestService.GetUserServiceRequestsAsync(userId.Value);
        return Ok(serviceRequests);
    }

    [HttpPost("{id}/payment")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment(int id, [FromBody] PaymentRequest paymentRequest)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _serviceRequestService.ProcessPaymentAsync(id, paymentRequest);
        
        if (result.IsSuccessful)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpGet("prices")]
    [AllowAnonymous]
    public async Task<ActionResult> GetServicePrices()
    {
        var indigenePrice = await _serviceRequestService.GetServicePriceAsync(Core.Entities.ServiceType.IndigeneCertificate);
        var statePrice = await _serviceRequestService.GetServicePriceAsync(Core.Entities.ServiceType.StateOfOrigin);
        var lgaPrice = await _serviceRequestService.GetServicePriceAsync(Core.Entities.ServiceType.LocalGovernmentCertificate);

        return Ok(new
        {
            IndigeneCertificate = indigenePrice,
            StateOfOrigin = statePrice,
            LocalGovernmentCertificate = lgaPrice
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
