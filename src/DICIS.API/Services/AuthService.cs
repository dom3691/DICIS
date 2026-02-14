using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DICIS.API.Services;

public class AuthService : IAuthService
{
    private readonly DicisDbContext _context;
    private readonly IConfiguration _configuration;
    private static readonly Dictionary<string, (string otp, DateTime expiresAt)> _otpStore = new();
    private static readonly object _otpLock = new object();

    public AuthService(DicisDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<NINVerifyResponse> VerifyNINAsync(string nin)
    {
        // Mock NIN verification - in production, integrate with NIMC API
        // For now, accept any 11-digit NIN
        if (string.IsNullOrEmpty(nin) || nin.Length != 11 || !nin.All(char.IsDigit))
        {
            return new NINVerifyResponse
            {
                IsValid = false,
                Message = "Invalid NIN format"
            };
        }

        // Mock data - in production, fetch from NIMC
        return new NINVerifyResponse
        {
            IsValid = true,
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Middle",
            Email = $"user{nin}@example.com",
            PhoneNumber = "08012345678",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Gender = "M"
        };
    }

    public async Task<string> GenerateOTPAsync(string nin, string phoneNumber, string email)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        lock (_otpLock)
        {
            _otpStore[nin] = (otp, expiresAt);
        }

        Console.WriteLine($"OTP for NIN {nin}: {otp}");
        return otp;
    }

    public async Task<OTPVerifyResponse> VerifyOTPAsync(string nin, string otp)
    {
        try
        {
            var response = new OTPVerifyResponse();
            (string storedOtp, DateTime expiresAt) otpData;

            lock (_otpLock)
            {
                if (!_otpStore.TryGetValue(nin, out otpData))
                {
                    response.IsValid = false;
                    response.Message = "OTP not found or expired";
                    return response;
                }
            }

            if (DateTime.UtcNow > otpData.expiresAt)
            {
                lock (_otpLock)
                {
                    _otpStore.Remove(nin);
                }
                response.IsValid = false;
                response.Message = "OTP expired";
                return response;
            }

            if (otpData.storedOtp != otp)
            {
                response.IsValid = false;
                response.Message = "Invalid OTP";
                return response;
            }

            lock (_otpLock)
            {
                _otpStore.Remove(nin);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NIN == nin);

            if (user == null)
            {
                var ninData = await VerifyNINAsync(nin);
                user = new User
                {
                    NIN = nin,
                    FirstName = ninData.FirstName ?? "Unknown",
                    LastName = ninData.LastName ?? "Unknown",
                    MiddleName = ninData.MiddleName,
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

            response.Token = GenerateJwtToken(user, Role.User);
            response.IsValid = true;
            response.Role = Role.User;
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
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> IsNINRegisteredAsync(string nin)
    {
        return await _context.Users.AnyAsync(u => u.NIN == nin);
    }

    public async Task<AdminLoginResponse> AdminLoginAsync(string username, string password)
    {
        var admin = await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

        if (admin == null)
        {
            return new AdminLoginResponse
            {
                IsValid = false,
                Message = "Invalid username or password"
            };
        }

        // Verify password
        if (!VerifyPassword(password, admin.PasswordHash))
        {
            return new AdminLoginResponse
            {
                IsValid = false,
                Message = "Invalid username or password"
            };
        }

        // Update last login
        admin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate token
        var token = GenerateJwtToken(admin);

        return new AdminLoginResponse
        {
            IsValid = true,
            Token = token,
            AdminUser = new AdminUserDTO
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                Role = admin.Role,
                State = admin.State,
                LGA = admin.LGA
            }
        };
    }

    public async Task<bool> LogoutAsync(string token)
    {
        // In a production system, you might want to blacklist the token
        // For now, we'll just return true as token invalidation is handled client-side
        return await Task.FromResult(true);
    }

    private string GenerateJwtToken(User user, Role role)
    {
        var secret = _configuration["Jwt:Secret"] ?? "your-secret-key-change-in-production-min-32-chars-long-for-security";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("nin", user.NIN),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateJwtToken(AdminUser admin)
    {
        var secret = _configuration["Jwt:Secret"] ?? "your-secret-key-change-in-production-min-32-chars-long-for-security";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, admin.Role.ToString())
        };

        if (!string.IsNullOrEmpty(admin.State))
        {
            claims.Add(new Claim("State", admin.State));
        }

        if (!string.IsNullOrEmpty(admin.LGA))
        {
            claims.Add(new Claim("LGA", admin.LGA));
        }

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == passwordHash;
    }
}
