using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DICIS.API.Services;

public class ProfileService : IProfileService
{
    private readonly DicisDbContext _context;
    private readonly IConfiguration _configuration;

    public ProfileService(DicisDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ProfileDTO> CreateProfileAsync(int userId, CreateProfileRequest request)
    {
        // Check if profile already exists
        var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existingProfile != null)
        {
            throw new InvalidOperationException("Profile already exists for this user");
        }

        // Check if email is already used
        var emailExists = await _context.UserProfiles.AnyAsync(p => p.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Email is already registered");
        }

        // Generate email verification token
        var verificationToken = GenerateVerificationToken();
        var tokenExpires = DateTime.UtcNow.AddDays(7);

        var profile = new UserProfile
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Country = request.Country,
            State = request.State,
            LocalGovernment = request.LocalGovernment,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            NIN = request.NIN,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpires = tokenExpires,
            IsProfileComplete = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Send verification email (mocked for now)
        await SendVerificationEmailAsync(request.Email, userId);

        return MapToDTO(profile);
    }

    public async Task<ProfileDTO?> GetProfileAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        return profile != null ? MapToDTO(profile) : null;
    }

    public async Task<ProfileDTO> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            throw new InvalidOperationException("Profile not found");
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            profile.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName))
            profile.LastName = request.LastName;
        if (request.MiddleName != null)
            profile.MiddleName = request.MiddleName;
        if (!string.IsNullOrEmpty(request.PhoneNumber))
            profile.PhoneNumber = request.PhoneNumber;
        if (request.Address != null)
            profile.Address = request.Address;
        if (request.City != null)
            profile.City = request.City;
        if (request.PostalCode != null)
            profile.PostalCode = request.PostalCode;

        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDTO(profile);
    }

    public async Task<bool> SendVerificationEmailAsync(string email, int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId && p.Email == email);
        if (profile == null)
        {
            return false;
        }

        // Generate new token if expired
        if (profile.EmailVerificationTokenExpires == null || profile.EmailVerificationTokenExpires < DateTime.UtcNow)
        {
            profile.EmailVerificationToken = GenerateVerificationToken();
            profile.EmailVerificationTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
        }

        // In production, send actual email
        // For now, log the verification link
        var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7001";
        var verificationLink = $"{baseUrl}/verify-email?token={profile.EmailVerificationToken}";
        
        Console.WriteLine($"Email verification link for {email}: {verificationLink}");
        
        // TODO: Integrate with email service (SendGrid, SMTP, etc.)
        
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.EmailVerificationToken == token);

        if (profile == null)
        {
            return false;
        }

        if (profile.EmailVerificationTokenExpires < DateTime.UtcNow)
        {
            return false; // Token expired
        }

        profile.IsEmailVerified = true;
        profile.EmailVerifiedAt = DateTime.UtcNow;
        profile.EmailVerificationToken = null;
        profile.EmailVerificationTokenExpires = null;
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Email == email);
        if (profile == null)
        {
            return false;
        }

        return await SendVerificationEmailAsync(email, profile.UserId);
    }

    public async Task<bool> IsProfileCompleteAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        return profile?.IsProfileComplete ?? false;
    }

    public async Task<bool> IsEmailVerifiedAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        return profile?.IsEmailVerified ?? false;
    }

    private string GenerateVerificationToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private ProfileDTO MapToDTO(UserProfile profile)
    {
        return new ProfileDTO
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            MiddleName = profile.MiddleName,
            Country = profile.Country,
            State = profile.State,
            LocalGovernment = profile.LocalGovernment,
            Email = profile.Email,
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            Address = profile.Address,
            City = profile.City,
            PostalCode = profile.PostalCode,
            NIN = profile.NIN,
            IsEmailVerified = profile.IsEmailVerified,
            IsProfileComplete = profile.IsProfileComplete,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
