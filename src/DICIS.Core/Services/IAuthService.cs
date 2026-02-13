using DICIS.Core.DTOs;

namespace DICIS.Core.Services;

public interface IAuthService
{
    Task<NINVerifyResponse> VerifyNINAsync(string nin);
    Task<OTPVerifyResponse> VerifyOTPAsync(string nin, string otp);
    Task<string> GenerateOTPAsync(string nin, string phoneNumber, string email);
    Task<bool> IsNINRegisteredAsync(string nin);
}
