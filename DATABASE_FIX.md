# Database Schema Fix

## Issue
The filtered index on `Applications` table was failing because:
- `ApplicationStatus` enum was stored as an integer
- The filter tried to compare it to string values like 'Approved'
- SQL Server cannot create filtered indexes with type mismatches

## Solution
All enums are now configured to be stored as strings in the database:
- `ApplicationStatus` → stored as string
- `CertificateStatus` → stored as string  
- `FraudReportStatus` → stored as string

## Action Required

Since the database was already created with the old schema, you need to **recreate the database**:

### Option 1: Delete and Recreate (Development - No Data Loss Concern)
1. Stop the API application if it's running
2. Delete the database:
   - Open SQL Server Management Studio
   - Connect to `(localdb)\mssqllocaldb`
   - Right-click on `DICIS` database → Delete
   - OR use command: `DROP DATABASE DICIS;`
3. Restart the API - it will recreate the database automatically

### Option 2: Use Migrations (Production - Preserves Data)
If you have existing data you want to preserve:

```bash
cd src/DICIS.API
dotnet ef migrations add FixEnumToStringConversion
dotnet ef database update
```

**Note**: The current code uses `EnsureCreated()` which drops and recreates the database. For production, you should:
1. Remove `EnsureCreated()` from `Program.cs`
2. Use migrations instead:
   ```csharp
   // Replace this:
   db.Database.EnsureCreated();
   
   // With this:
   db.Database.Migrate();
   ```

## Verification

After recreating the database, the filtered index should be created successfully:
- Index: `IX_Applications_UserId_State`
- Filter: `[Status] = 'Approved'`
- Purpose: Ensures one approved certificate per user per state

## Benefits of String Storage

1. **Readability**: Database values are human-readable ('Approved' vs 2)
2. **Filtered Indexes**: Can use string comparisons in filters
3. **Debugging**: Easier to query and debug in SQL
4. **Consistency**: All status fields use the same storage format
