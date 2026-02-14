# Database Schema Update - QRCodeData Column

## Issue
The `QRCodeData` column in the `Certificates` table was too small (500 characters) to store the base64-encoded QR code image data, which can be several thousand characters long.

## Fix Applied
- Increased `QRCodeData` column size from 500 to 5000 characters
- Updated Entity Framework configuration to reflect this change

## Action Required

Since the database was already created with the old schema, you need to **update the database**:

### Option 1: Delete and Recreate Database (Development - No Data Loss Concern)

1. **Stop the API** if it's running (Ctrl+C)

2. **Delete the database**:
   - Open SQL Server Management Studio
   - Connect to `(localdb)\mssqllocaldb`
   - Right-click on `DICIS` database → Delete
   - OR use command:
     ```sql
     DROP DATABASE DICIS;
     ```

3. **Restart the API** - it will recreate the database automatically with the new schema

### Option 2: Use SQL Script to Alter Column (Preserves Data)

If you have existing data you want to preserve:

```sql
USE DICIS;
GO

ALTER TABLE Certificates
ALTER COLUMN QRCodeData NVARCHAR(5000) NOT NULL;
GO
```

Then restart the API.

### Option 3: Use Entity Framework Migrations (Recommended for Production)

1. **Install EF Tools** (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. **Create a migration**:
   ```bash
   cd src/DICIS.API
   dotnet ef migrations add IncreaseQRCodeDataSize
   ```

3. **Apply the migration**:
   ```bash
   dotnet ef database update
   ```

## Verification

After updating the database, verify the column size:

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Certificates' 
AND COLUMN_NAME = 'QRCodeData';
```

Should show: `CHARACTER_MAXIMUM_LENGTH = 5000`

## Quick Fix (Development)

If you're in development and don't have important data:

1. Stop API
2. Delete database: `DROP DATABASE DICIS;`
3. Restart API (it will recreate with correct schema)

This is the fastest solution for development!
