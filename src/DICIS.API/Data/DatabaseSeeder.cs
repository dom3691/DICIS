using DICIS.Core.Data;
using DICIS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DICIS.API.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DicisDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Super Admin if not exists
        if (!await context.AdminUsers.AnyAsync(a => a.Role == Role.SuperAdmin))
        {
            var superAdmin = new AdminUser
            {
                Username = "superadmin",
                Email = "superadmin@dicis.gov.ng",
                PasswordHash = HashPassword("SuperAdmin@2024"), // Default password
                Role = Role.SuperAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.AdminUsers.Add(superAdmin);
            await context.SaveChangesAsync();
        }

        // You can add more seed data here
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
