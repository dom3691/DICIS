# Migration Fix - Database Already Exists

## Problem
The database was created using `EnsureCreated()` which doesn't use migrations. Now when trying to apply migrations, it fails because tables already exist.

## Solution Options

### Option 1: Delete and Recreate Database (Recommended for Development)

1. **Delete the existing database**:
   ```sql
   USE master;
   GO
   DROP DATABASE DICIS;
   GO
   ```

   Or use SQL Server Management Studio:
   - Right-click on `DICIS` database → Delete → Check "Close existing connections" → OK

2. **Apply the migration**:
   ```powershell
   Update-Database
   ```

   Or:
   ```bash
   dotnet ef database update
   ```

### Option 2: Remove EnsureCreated() and Use Migrations Only

1. **Remove EnsureCreated() from Program.cs** (if it exists)
2. **Delete the database** (as in Option 1)
3. **Apply migration**

### Option 3: Mark Migration as Applied (If Schema Matches)

If your existing database schema matches the migration, you can mark it as applied:

```powershell
# In Package Manager Console
Update-Database -Script
```

Then manually insert into `__EFMigrationsHistory`:
```sql
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20260214000915_InitialMigration', '8.0.0');
```

## Recommended Approach

For development, **Option 1** is the cleanest:
1. Delete the database
2. Apply the migration
3. The seeder will automatically create the SuperAdmin user

## After Fixing

Once the migration is applied successfully:
- The database will be properly versioned
- Future schema changes can be managed through migrations
- The SuperAdmin user will be seeded automatically on startup
