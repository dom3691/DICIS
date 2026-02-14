# SSL Certificate Error Fix

## Problem
Getting SSL certificate error when connecting to SQL Server:
```
The certificate chain was issued by an authority that is not trusted.
```

## Solution
Added `TrustServerCertificate=True` to the connection string to bypass SSL certificate validation for local development.

## Changes Made

### 1. Updated `appsettings.json`
Added `TrustServerCertificate=True` to the connection string.

### 2. Updated `Program.cs`
Updated the default connection string fallback to include `TrustServerCertificate=True`.

## Connection String Format

**Before:**
```
Server=(localdb)\mssqllocaldb;Database=DICIS;Trusted_Connection=True;MultipleActiveResultSets=true
```

**After:**
```
Server=(localdb)\mssqllocaldb;Database=DICIS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

## Security Note

⚠️ **Important**: `TrustServerCertificate=True` bypasses SSL certificate validation. This is:
- ✅ **Safe for local development** (LocalDB)
- ⚠️ **NOT recommended for production** - Use proper SSL certificates in production

## Next Steps

1. **Restart the API** - The connection string is now fixed
2. **The seeder should run successfully** - SuperAdmin will be created
3. **Test the application** - Everything should work now

## Alternative Solutions (For Production)

If you need proper SSL in production:

1. **Install a valid SSL certificate** on the SQL Server
2. **Remove `TrustServerCertificate=True`** from connection string
3. **Ensure the certificate is trusted** by the client machine

For now, the fix is applied and should resolve the connection issue!
