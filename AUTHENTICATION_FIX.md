# Authentication 401 Error Fix

## Problem
Getting `401 Unauthorized` when trying to submit an application because:
1. JWT token was not properly generated (was just base64, not a real JWT)
2. Token wasn't being sent correctly in HTTP requests

## Fixes Applied

### 1. Fixed JWT Token Generation
- **Before**: Simple base64 encoding (not a real JWT)
- **After**: Proper JWT token using `JwtSecurityTokenHandler`
- Uses proper signing with HMAC SHA256
- Includes proper claims (NameIdentifier, NIN, Name, Email)

### 2. Improved Token Handling in Blazor
- Added proper header removal before setting new token
- Ensures token is sent with every request

## What You Need to Do

### Step 1: Restart the API
The API needs to be restarted to apply the JWT token generation fix:

1. **Stop the API** (Ctrl+C in the terminal)
2. **Restart it**:
   ```bash
   cd src/DICIS.API
   dotnet run
   ```

### Step 2: Login Again
Since the token format changed, you need to login again:

1. Go to `/login`
2. Enter your NIN
3. Get the OTP (from the message or console)
4. Enter the OTP
5. You'll get a **proper JWT token** now

### Step 3: Try Submitting Application Again
After logging in with the new token format, try submitting your application again.

## Verification

To verify the token is working:

1. **Check browser console** (F12 → Application/Storage → Local Storage)
   - Look for `authToken` key
   - The value should be a long JWT token (starts with `eyJ...`)

2. **Check Network tab** when submitting application:
   - Look at the request headers
   - Should see: `Authorization: Bearer eyJ...`

## If Still Getting 401

1. **Clear browser storage**:
   - F12 → Application → Local Storage → Clear all
   - Or manually delete `authToken`

2. **Login again** to get a new token

3. **Check API logs** for authentication errors

4. **Verify JWT secret** in `appsettings.json` matches (should be at least 32 characters)

## Token Format

**Old (broken)**: Base64 string (not a real JWT)
**New (working)**: Proper JWT token like:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmluIjoiNzgzNzgxMjcxODciLCJuYW1lIjoiSm9obiBEb2UiLCJlbWFpbCI6InVzZXI3ODM3ODEyNzE4N0BleGFtcGxlLmNvbSIsImV4cCI6MTcwNzg5NjAwMH0.xxxxx
```

The new token will work with JWT Bearer authentication!
