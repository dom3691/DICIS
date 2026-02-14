# Code Restructuring Summary

## Overview
The codebase has been restructured following best practices with modular architecture, role-based access control, and proper database migrations.

## Key Changes

### 1. **Role-Based Access Control (RBAC)**
- Created `Role` enum with two roles:
  - `User` (0) - Base user role for citizens
  - `SuperAdmin` (1) - Super administrator with full system access
- Updated `AdminUser` entity to use `Role` enum instead of string
- Configured role-based authorization policies in `Program.cs`

### 2. **Authentication & Authorization**
- **Enhanced AuthService**:
  - Added `AdminLoginAsync()` for admin authentication
  - Added `LogoutAsync()` for token invalidation
  - JWT tokens now include role claims
  - Password hashing using SHA256

- **Updated AuthController**:
  - Added `/api/auth/admin/login` endpoint
  - Added `/api/auth/logout` endpoint
  - Proper error handling and validation

### 3. **Database Structure**
- **AdminUser Table**:
  - `Id` (Primary Key)
  - `Username` (Unique)
  - `Email` (Unique)
  - `PasswordHash` (SHA256 hashed)
  - `Role` (Enum stored as string: "User" or "SuperAdmin")
  - `State` (Optional, for state-specific admins)
  - `LGA` (Optional, for LGA-specific admins)
  - `IsActive` (Boolean)
  - `CreatedAt` (DateTime)
  - `LastLoginAt` (Nullable DateTime)

- **All Tables**:
  - Users
  - Applications
  - Certificates
  - AuditLogs
  - AdminUsers
  - FraudReports

### 4. **Database Seeding**
- Created `DatabaseSeeder` class
- Automatically seeds SuperAdmin user on startup:
  - **Username**: `superadmin`
  - **Email**: `superadmin@dicis.gov.ng`
  - **Password**: `SuperAdmin@2024` (change in production!)
  - **Role**: `SuperAdmin`

### 5. **Blazor UI Updates**
- **Login Page** (`Login.razor`):
  - Toggle between Citizen Login (NIN + OTP) and Admin Login (Username + Password)
  - Proper error handling and loading states
  - Role-based navigation after login

- **MainLayout** (`MainLayout.razor`):
  - Shows current user role
  - Logout button when authenticated
  - Login button when not authenticated

- **AuthService** (Blazor):
  - `AdminLoginAsync()` - Admin login
  - `IsSuperAdminAsync()` - Check if user is super admin
  - `GetUserRoleAsync()` - Get current user role
  - `LogoutAsync()` - Proper logout with token cleanup

### 6. **DTOs Updated**
- Added `AdminLoginRequest` and `AdminLoginResponse`
- Added `AdminUserDTO`
- Updated `OTPVerifyResponse` to include `Role`
- Updated `NINVerifyResponse` with missing fields

## Database Migration

### To Create Migration:
```bash
cd src/DICIS.API
dotnet ef migrations add InitialCreate
```

### To Apply Migration:
```bash
dotnet ef database update
```

### Or Delete and Recreate (Development):
```sql
DROP DATABASE DICIS;
```
Then restart the API - it will auto-create with seed data.

## Super Admin Access

### Default Credentials:
- **Username**: `superadmin`
- **Password**: `SuperAdmin@2024`

⚠️ **IMPORTANT**: Change the default password in production!

### Super Admin Permissions:
- Full access to all system resources
- Can access admin dashboard
- Can manage all applications
- Can review exceptions
- Can revoke certificates
- Can view all analytics

## Code Structure

```
src/
├── DICIS.Core/
│   ├── Entities/
│   │   ├── Role.cs (NEW)
│   │   ├── AdminUser.cs (UPDATED)
│   │   └── ...
│   ├── DTOs/
│   │   └── AuthDTOs.cs (UPDATED)
│   ├── Services/
│   │   └── IAuthService.cs (UPDATED)
│   └── Data/
│       └── DicisDbContext.cs (UPDATED)
├── DICIS.API/
│   ├── Services/
│   │   └── AuthService.cs (UPDATED)
│   ├── Controllers/
│   │   └── AuthController.cs (UPDATED)
│   ├── Data/
│   │   └── DatabaseSeeder.cs (NEW)
│   └── Program.cs (UPDATED)
└── DICIS.Blazor/
    ├── Services/
    │   └── AuthService.cs (UPDATED)
    ├── Pages/
    │   └── Login.razor (UPDATED)
    └── Shared/
        └── MainLayout.razor (UPDATED)
```

## Authorization Policies

### In Program.cs:
- `SuperAdminOnly` - Requires SuperAdmin role
- `AdminOrSuperAdmin` - Requires either SuperAdmin or User role

### Usage in Controllers:
```csharp
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    // Only SuperAdmin can access
}
```

## Next Steps

1. **Stop the running API** (if running)
2. **Create migration**: `dotnet ef migrations add InitialCreate`
3. **Apply migration**: `dotnet ef database update`
4. **Restart API** - SuperAdmin will be seeded automatically
5. **Test login**:
   - Citizen: Use NIN + OTP
   - Admin: Use `superadmin` / `SuperAdmin@2024`

## Security Notes

1. **Password Hashing**: Currently using SHA256. Consider upgrading to bcrypt or Argon2 in production.
2. **JWT Secret**: Ensure it's at least 32 characters and stored securely.
3. **Default Password**: Change SuperAdmin password immediately after first login.
4. **HTTPS**: Always use HTTPS in production.
5. **Token Expiry**: Currently 24 hours. Adjust based on security requirements.

## Testing

### Test Citizen Login:
1. Navigate to `/login`
2. Select "Citizen Login"
3. Enter NIN (11 digits)
4. Enter OTP from console/message
5. Should redirect to home page

### Test Admin Login:
1. Navigate to `/login`
2. Select "Admin Login"
3. Enter username: `superadmin`
4. Enter password: `SuperAdmin@2024`
5. Should redirect to admin dashboard

### Test Logout:
1. Click logout button in header
2. Should clear token and redirect to login

## Troubleshooting

### Migration Issues:
- If migration fails, delete database and let it recreate
- Ensure SQL Server is running
- Check connection string in `appsettings.json`

### Login Issues:
- Check console for OTP (development mode)
- Verify SuperAdmin was seeded (check database)
- Check JWT secret in configuration

### Authorization Issues:
- Verify role is included in JWT token
- Check authorization policies in Program.cs
- Ensure `[Authorize]` attributes are correct
