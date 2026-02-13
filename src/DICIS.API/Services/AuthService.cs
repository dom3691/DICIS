using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DICIS.API.Services;

public class AuthService : IAuthService
{
    private readonly DicisDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, (string otp, DateTime expiresAt)> _otpStore = new();

    public AuthService(DicisDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<NINVerifyResponse> VerifyNINAsync(string nin)
    {
        // In production, this would call NIMC API
        // For now, simulate NIMC verification
        var response = new NINVerifyResponse();
        
        // Simulate NIMC API call
        if (nin.Length == 11 && nin.All(char.IsDigit))
        {
            response.IsValid = true;
            response.PhoneNumber = "08012345678"; // Mock data
            response.Email = $"user{nin}@example.com"; // Mock data
            response.FirstName = "John"; // Mock data
            response.LastName = "Doe"; // Mock data
            response.Message = "NIN verified successfully";
        }
        else
        {
            response.IsValid = false;
            response.Message = "Invalid NIN format";
        }
        
        return response;
    }

    public async Task<string> GenerateOTPAsync(string nin, string phoneNumber, string email)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        
        _otpStore[nin] = (otp, expiresAt);
        
        // In production, send OTP via SMS/Email
        // For now, log it (in production, use SMS/Email service)
        Console.WriteLine($"OTP for NIN {nin}: {otp}");
        
        return otp;
    }

    public async Task<OTPVerifyResponse> VerifyOTPAsync(string nin, string otp)
    {
        var response = new OTPVerifyResponse();
        
        if (!_otpStore.ContainsKey(nin))
        {
            response.IsValid = false;
            response.Message = "OTP not found or expired";
            return response;
        }
        
        var (storedOtp, expiresAt) = _otpStore[nin];
        
        if (DateTime.UtcNow > expiresAt)
        {
            _otpStore.Remove(nin);
            response.IsValid = false;
            response.Message = "OTP expired";
            return response;
        }
        
        if (storedOtp != otp)
        {
            response.IsValid = false;
            response.Message = "Invalid OTP";
            return response;
        }
        
        // OTP is valid
        _otpStore.Remove(nin);
        
        // Get or create user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.NIN == nin);
        
        if (user == null)
        {
            // Create new user (in production, get from NIMC API)
            var ninData = await VerifyNINAsync(nin);
            user = new User
            {
                NIN = nin,
                FirstName = ninData.FirstName ?? "Unknown",
                LastName = ninData.LastName ?? "Unknown",
                Email = ninData.Email ?? $"{nin}@example.com",
                PhoneNumber = ninData.PhoneNumber ?? "00000000000",
                DateOfBirth = DateTime.UtcNow.AddYears(-25), // Mock
                Gender = "M",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        // Generate JWT token
        response.Token = GenerateJwtToken(user);
        response.IsValid = true;
        response.User = new UserDTO
        {
            Id = user.Id,
            NIN = user.NIN,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
        
        return response;
    }

    public async Task<bool> IsNINRegisteredAsync(string nin)
    {
        return await _context.Users.AnyAsync(u => u.NIN == nin);
    }

    private string GenerateJwtToken(User user)
    {
        // Simplified JWT generation (in production, use proper JWT library)
        var secret = _configuration["Jwt:Secret"] ?? "your-secret-key-change-in-production";
        var payload = $"{{\"sub\":\"{user.Id}\",\"nin\":\"{user.NIN}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()}}}";
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        return token;
    }
}
