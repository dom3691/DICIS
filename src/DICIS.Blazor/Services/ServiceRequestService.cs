using Blazored.LocalStorage;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using System.Net.Http.Json;

namespace DICIS.Blazor.Services;

public class ServiceRequestService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ServiceRequestService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<ServiceRequestDTO> CreateServiceRequestAsync(CreateServiceRequestDTO request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/servicerequest", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ServiceRequestDTO>() ?? new ServiceRequestDTO();
    }

    public async Task<ServiceRequestDTO?> GetServiceRequestAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/servicerequest/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ServiceRequestDTO>();
    }

    public async Task<List<ServiceRequestDTO>> GetServiceRequestsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/servicerequest");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ServiceRequestDTO>>() ?? new List<ServiceRequestDTO>();
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(int serviceRequestId, PaymentRequest paymentRequest)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/servicerequest/{serviceRequestId}/payment", paymentRequest);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PaymentResponse>() ?? new PaymentResponse();
        }
        
        var error = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        return error ?? new PaymentResponse { IsSuccessful = false, Message = "Payment failed" };
    }

    public async Task<Dictionary<string, decimal>> GetServicePricesAsync()
    {
        var response = await _httpClient.GetAsync("api/servicerequest/prices");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Dictionary<string, decimal>>() ?? new Dictionary<string, decimal>();
    }
}
