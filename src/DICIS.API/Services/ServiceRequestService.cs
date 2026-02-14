using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DICIS.API.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly DicisDbContext _context;
    private readonly IConfiguration _configuration;

    public ServiceRequestService(DicisDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceRequestDTO> CreateServiceRequestAsync(int userId, CreateServiceRequestDTO request)
    {
        // Get service price
        var amount = await GetServicePriceAsync(request.ServiceType);

        var serviceRequest = new ServiceRequest
        {
            UserId = userId,
            ServiceType = request.ServiceType,
            Status = ServiceRequestStatus.Pending,
            Amount = amount,
            PaymentStatus = PaymentStatus.Pending,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        return MapToDTO(serviceRequest);
    }

    public async Task<ServiceRequestDTO?> GetServiceRequestAsync(int serviceRequestId, int userId)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(s => s.Application)
            .Include(s => s.Certificate)
            .FirstOrDefaultAsync(s => s.Id == serviceRequestId && s.UserId == userId);

        return serviceRequest != null ? MapToDTO(serviceRequest) : null;
    }

    public async Task<List<ServiceRequestDTO>> GetUserServiceRequestsAsync(int userId)
    {
        var serviceRequests = await _context.ServiceRequests
            .Include(s => s.Application)
            .Include(s => s.Certificate)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return serviceRequests.Select(MapToDTO).ToList();
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(int serviceRequestId, PaymentRequest paymentRequest)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(s => s.Id == serviceRequestId);

        if (serviceRequest == null)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                Message = "Service request not found"
            };
        }

        if (serviceRequest.PaymentStatus == PaymentStatus.Completed)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                Message = "Payment already completed"
            };
        }

        // Mock payment processing
        // In production, integrate with payment gateway (Paystack, Flutterwave, etc.)
        var paymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{serviceRequestId}";
        
        // Simulate payment processing delay
        await Task.Delay(1000);

        // Mock successful payment (90% success rate for demo)
        var random = new Random();
        var isSuccessful = random.Next(1, 11) <= 9; // 90% success

        if (isSuccessful)
        {
            serviceRequest.PaymentStatus = PaymentStatus.Completed;
            serviceRequest.PaymentDate = DateTime.UtcNow;
            serviceRequest.PaymentReference = paymentReference;
            serviceRequest.PaymentDetails = JsonSerializer.Serialize(new
            {
                Method = paymentRequest.PaymentMethod,
                Reference = paymentReference,
                Amount = serviceRequest.Amount,
                Status = "Success",
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                IsSuccessful = true,
                PaymentReference = paymentReference,
                Message = "Payment processed successfully",
                PaymentDate = DateTime.UtcNow
            };
        }
        else
        {
            serviceRequest.PaymentStatus = PaymentStatus.Failed;
            serviceRequest.PaymentDetails = JsonSerializer.Serialize(new
            {
                Method = paymentRequest.PaymentMethod,
                Status = "Failed",
                Message = "Payment declined"
            });

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                IsSuccessful = false,
                Message = "Payment failed. Please try again."
            };
        }
    }

    public async Task<ServiceRequestDTO> CompleteServiceRequestAsync(int serviceRequestId)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(s => s.Id == serviceRequestId);

        if (serviceRequest == null)
        {
            throw new InvalidOperationException("Service request not found");
        }

        serviceRequest.Status = ServiceRequestStatus.Completed;
        serviceRequest.CompletedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return MapToDTO(serviceRequest);
    }

    public async Task<decimal> GetServicePriceAsync(ServiceType serviceType)
    {
        // Service prices (can be moved to configuration)
        return serviceType switch
        {
            ServiceType.IndigeneCertificate => 5000.00m,
            ServiceType.StateOfOrigin => 3000.00m,
            ServiceType.LocalGovernmentCertificate => 2000.00m,
            _ => 1000.00m
        };
    }

    private ServiceRequestDTO MapToDTO(ServiceRequest serviceRequest)
    {
        return new ServiceRequestDTO
        {
            Id = serviceRequest.Id,
            UserId = serviceRequest.UserId,
            ServiceType = serviceRequest.ServiceType,
            Status = serviceRequest.Status,
            Amount = serviceRequest.Amount,
            PaymentReference = serviceRequest.PaymentReference,
            PaymentStatus = serviceRequest.PaymentStatus,
            PaymentDate = serviceRequest.PaymentDate,
            Notes = serviceRequest.Notes,
            CreatedAt = serviceRequest.CreatedAt,
            CompletedAt = serviceRequest.CompletedAt,
            Application = serviceRequest.Application != null ? new ApplicationDTO
            {
                Id = serviceRequest.Application.Id,
                State = serviceRequest.Application.State,
                LGA = serviceRequest.Application.LGA,
                Status = serviceRequest.Application.Status,
                CreatedAt = serviceRequest.Application.CreatedAt,
                SubmittedAt = serviceRequest.Application.SubmittedAt,
                ApprovedAt = serviceRequest.Application.ApprovedAt
            } : null,
            Certificate = serviceRequest.Certificate != null ? new CertificateDTO
            {
                CertificateId = serviceRequest.Certificate.CertificateId,
                Status = serviceRequest.Certificate.Status,
                IssuedAt = serviceRequest.Certificate.IssuedAt,
                ExpiresAt = serviceRequest.Certificate.ExpiresAt
            } : null
        };
    }
}
