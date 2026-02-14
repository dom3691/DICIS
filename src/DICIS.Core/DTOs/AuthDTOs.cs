using DICIS.Core.Entities;

namespace DICIS.Core.DTOs;

public class NINVerifyRequest
{
    public string NIN { get; set; } = string.Empty;
}

public class NINVerifyResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}

public class OTPVerifyRequest
{
    public string NIN { get; set; } = string.Empty;
    public string OTP { get; set; } = string.Empty;
}

public class OTPVerifyResponse
{
    public bool IsValid { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public UserDTO? User { get; set; }
    public Role? Role { get; set; }
}

public class UserDTO
{
    public int Id { get; set; }
    public string NIN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AdminLoginResponse
{
    public bool IsValid { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public AdminUserDTO? AdminUser { get; set; }
}

public class AdminUserDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string? State { get; set; }
    public string? LGA { get; set; }
}
