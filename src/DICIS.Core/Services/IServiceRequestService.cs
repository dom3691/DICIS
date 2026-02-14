using DICIS.Core.DTOs;
using DICIS.Core.Entities;

namespace DICIS.Core.Services;

public interface IServiceRequestService
{
    Task<ServiceRequestDTO> CreateServiceRequestAsync(int userId, CreateServiceRequestDTO request);
    Task<ServiceRequestDTO?> GetServiceRequestAsync(int serviceRequestId, int userId);
    Task<List<ServiceRequestDTO>> GetUserServiceRequestsAsync(int userId);
    Task<PaymentResponse> ProcessPaymentAsync(int serviceRequestId, PaymentRequest paymentRequest);
    Task<ServiceRequestDTO> CompleteServiceRequestAsync(int serviceRequestId);
    Task<decimal> GetServicePriceAsync(ServiceType serviceType);
}
