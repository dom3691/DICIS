using Blazored.LocalStorage;
using DICIS.Core.DTOs;
using System.Net.Http.Json;

namespace DICIS.Blazor.Services;

public class ProfileService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ProfileService(HttpClient httpClient, ILocalStorageService localStorage)
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

    public async Task<ProfileDTO?> CreateProfileAsync(CreateProfileRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/profile", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProfileDTO>();
    }

    public async Task<ProfileDTO?> GetProfileAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/profile");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProfileDTO>();
    }

    public async Task<ProfileDTO> UpdateProfileAsync(UpdateProfileRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/profile", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProfileDTO>() ?? new ProfileDTO();
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var response = await _httpClient.PostAsJsonAsync("api/profile/verify-email", new VerifyEmailRequest { Token = token });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        var response = await _httpClient.PostAsJsonAsync("api/profile/resend-verification", new ResendVerificationEmailRequest { Email = email });
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool IsComplete, bool IsVerified)> CheckProfileStatusAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/profile/check-complete");
        if (!response.IsSuccessStatusCode)
        {
            return (false, false);
        }
        
        var status = await response.Content.ReadFromJsonAsync<ProfileStatusResponse>();
        return (status?.IsProfileComplete ?? false, status?.IsEmailVerified ?? false);
    }
}

public class ProfileStatusResponse
{
    public bool IsProfileComplete { get; set; }
    public bool IsEmailVerified { get; set; }
}
