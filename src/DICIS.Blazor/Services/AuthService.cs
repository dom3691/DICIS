using Blazored.LocalStorage;
using DICIS.Core.DTOs;
using System.Net.Http.Json;

namespace DICIS.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<NINVerifyResponse> VerifyNINAsync(string nin)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/nin-verify", new NINVerifyRequest { NIN = nin });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<NINVerifyResponse>() ?? new NINVerifyResponse();
    }

    public async Task<OTPVerifyResponse> VerifyOTPAsync(string nin, string otp)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/otp-verify", new OTPVerifyRequest { NIN = nin, OTP = otp });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OTPVerifyResponse>();
            if (result?.IsValid == true && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            }
            return result ?? new OTPVerifyResponse();
        }
        
        return new OTPVerifyResponse { IsValid = false, Message = "Invalid OTP" };
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return true;
        }
        return false;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
