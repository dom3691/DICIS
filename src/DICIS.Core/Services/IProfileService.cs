using DICIS.Core.DTOs;

namespace DICIS.Core.Services;

public interface IProfileService
{
    Task<ProfileDTO> CreateProfileAsync(int userId, CreateProfileRequest request);
    Task<ProfileDTO?> GetProfileAsync(int userId);
    Task<ProfileDTO> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<bool> SendVerificationEmailAsync(string email, int userId);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
    Task<bool> IsProfileCompleteAsync(int userId);
    Task<bool> IsEmailVerifiedAsync(int userId);
}
