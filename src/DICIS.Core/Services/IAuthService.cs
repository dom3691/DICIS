using DICIS.Core.DTOs;

namespace DICIS.Core.Services;

public interface IAuthService
{
    Task<NINVerifyResponse> VerifyNINAsync(string nin);
    Task<string> GenerateOTPAsync(string nin, string phoneNumber, string email);
    Task<OTPVerifyResponse> VerifyOTPAsync(string nin, string otp);
    Task<bool> IsNINRegisteredAsync(string nin);
    Task<AdminLoginResponse> AdminLoginAsync(string username, string password);
    Task<bool> LogoutAsync(string token);
}
