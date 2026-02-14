# Apply Migration - Step by Step

## ✅ Step 1: Database Deleted
The database has been successfully deleted.

## Step 2: Apply Migration

Now run in **Package Manager Console**:

```powershell
Update-Database
```

Or using command line:

```bash
cd src/DICIS.API
dotnet ef database update
```

## Expected Result

You should see:
- ✅ Migration applied successfully
- ✅ All tables created
- ✅ SuperAdmin user seeded automatically

## If You Still Get Errors

If you still see "table already exists" errors:

1. **Check if database was fully deleted**:
   ```sql
   SELECT name FROM sys.databases WHERE name = 'DICIS';
   ```
   Should return no rows.

2. **Manually delete if needed**:
   - Open SQL Server Management Studio
   - Connect to `(localdb)\mssqllocaldb`
   - If DICIS database exists, right-click → Delete → Check "Close existing connections" → OK

3. **Then run Update-Database again**

## After Migration

Once successful:
- Database will be properly versioned
- SuperAdmin user will be created automatically
- You can login with:
  - Username: `superadmin`
  - Password: `SuperAdmin@2024`
