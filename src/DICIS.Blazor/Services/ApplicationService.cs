using DICIS.Core.DTOs;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace DICIS.Blazor.Services;

public class ApplicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApplicationService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            // Remove existing authorization header if any
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            // Add new authorization header
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            // If no token, remove authorization header
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }
    }

    public async Task<ApplicationDTO> CreateApplicationAsync(ApplicationCreateRequest request)
    {
        await SetAuthHeaderAsync();
        
        // Verify token is set
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Not authenticated. Please login first.");
        }
        
        var response = await _httpClient.PostAsJsonAsync("api/applications", request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Token might be invalid, clear it
            await _localStorage.RemoveItemAsync("authToken");
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        }
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationDTO>() ?? new ApplicationDTO();
    }

    public async Task<List<ApplicationDTO>> GetApplicationsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/applications");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ApplicationDTO>>() ?? new List<ApplicationDTO>();
    }

    public async Task<ApplicationDTO> GetApplicationAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/applications/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationDTO>() ?? new ApplicationDTO();
    }

    public async Task<ApplicationDTO> SubmitApplicationAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/applications/{id}/submit", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationDTO>() ?? new ApplicationDTO();
    }

    public async Task<CertificateVerifyResponse> VerifyCertificateAsync(string certificateId)
    {
        var response = await _httpClient.GetAsync($"api/certificate/verify/{certificateId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CertificateVerifyResponse>() ?? new CertificateVerifyResponse();
    }
}
