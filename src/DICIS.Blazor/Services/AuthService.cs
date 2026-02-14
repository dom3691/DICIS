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
                await _localStorage.SetItemAsync("userRole", result.Role?.ToString() ?? "User");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            }
            return result ?? new OTPVerifyResponse();
        }
        
        return new OTPVerifyResponse { IsValid = false, Message = "Invalid OTP" };
    }

    public async Task<AdminLoginResponse> AdminLoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/admin/login", 
            new AdminLoginRequest { Username = username, Password = password });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
            if (result?.IsValid == true && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _localStorage.SetItemAsync("userRole", result.AdminUser?.Role.ToString() ?? "User");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            }
            return result ?? new AdminLoginResponse();
        }
        
        return new AdminLoginResponse { IsValid = false, Message = "Invalid credentials" };
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetUserRoleAsync()
    {
        return await _localStorage.GetItemAsync<string>("userRole");
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        var role = await GetUserRoleAsync();
        return role == "SuperAdmin";
    }

    public async Task LogoutAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                await _httpClient.PostAsync("api/auth/logout", null);
            }
        }
        catch
        {
            // Ignore errors during logout
        }
        finally
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userRole");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
