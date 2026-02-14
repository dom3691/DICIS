# Foreign Key Constraint Fix

## Problem
When a SuperAdmin tried to create an application, it failed with:
```
The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Applications_Users_UserId"
```

## Root Cause
- SuperAdmin logged in → JWT token contains AdminUser.Id = 1
- Application tried to use UserId = 1 (from AdminUser)
- But UserId = 1 doesn't exist in Users table (it's in AdminUsers table)
- Foreign key constraint violation!

## Solution Applied

### 1. Role-Based Access Control
- **Applications can only be created by regular Users (citizens)**, not AdminUsers
- Added role check in `CreateApplication` endpoint
- Returns clear error message if admin tries to create application

### 2. User Verification
- Added check to verify user exists in Users table before creating application
- Prevents foreign key violations

### 3. Admin Access to View Applications
- SuperAdmin can view ALL applications (for management)
- Regular users can only view their own applications

## Changes Made

### `CreateApplication` Endpoint:
- ✅ Checks if user is admin → returns error
- ✅ Verifies user exists in Users table
- ✅ Only allows regular Users to create applications

### `GetApplications` Endpoint:
- ✅ SuperAdmin sees all applications
- ✅ Regular users see only their own

### `GetApplication` Endpoint:
- ✅ SuperAdmin can view any application
- ✅ Regular users can only view their own

## How to Use

### For Citizens (Regular Users):
1. **Login with NIN + OTP** (creates/uses User in Users table)
2. **Create applications** - Works perfectly!

### For Admins:
1. **Login with username/password** (uses AdminUser)
2. **Cannot create applications** - Will get clear error message
3. **Can view all applications** - For management purposes
4. **Can review exceptions** - Via ExceptionReviewController

## Testing

### Test Citizen Flow:
1. Login as citizen (NIN + OTP)
2. Create application → Should work ✅

### Test Admin Flow:
1. Login as SuperAdmin
2. Try to create application → Should get error message ✅
3. View applications → Should see all applications ✅

## Summary

- ✅ Fixed foreign key constraint violation
- ✅ Proper role-based access control
- ✅ Clear error messages
- ✅ Admins can manage, citizens can apply

The system now properly separates:
- **Users** (citizens) → Can create applications
- **AdminUsers** (admins) → Can manage applications, cannot create them
