# Quick Start Guide

## Prerequisites
- .NET 8.0 SDK installed
- SQL Server LocalDB (comes with Visual Studio) or SQL Server instance
- A code editor (Visual Studio 2022, VS Code, or Rider)

## Step 1: Restore Packages

```bash
dotnet restore
```

## Step 2: Update Connection String

Edit `src/DICIS.API/appsettings.json` and update the connection string if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DICIS;Trusted_Connection=True;"
}
```

## Step 3: Update API URL in Blazor

Edit `src/DICIS.Blazor/Program.cs` and ensure the API base URL matches your API:

```csharp
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("https://localhost:7000") // or http://localhost:5000
});
```

## Step 4: Run the Backend API

Open a terminal in the project root:

```bash
cd src/DICIS.API
dotnet run
```

The API will start on:
- HTTPS: https://localhost:7000
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:7000/swagger

## Step 5: Run the Blazor Frontend

Open another terminal:

```bash
cd src/DICIS.Blazor
dotnet run
```

The Blazor app will start on:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001

## Step 6: Test the Application

1. Open your browser to `https://localhost:7001`
2. Click "Login" or navigate to `/login`
3. Enter a test NIN (11 digits, e.g., "12345678901")
4. Check the console output for the OTP (in production, this would be sent via SMS/Email)
5. Enter the OTP to login
6. Apply for a certificate
7. View your applications
8. Verify certificates using the verification portal

## Testing with Sample Data

### Test NINs
- Use any 11-digit number as NIN (e.g., "12345678901")
- The system will create mock user data

### Test OTP
- After entering NIN, check the console/terminal output for the OTP
- In production, OTPs are sent via SMS/Email

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server LocalDB is installed
- Try changing the connection string to use a full SQL Server instance
- Check that the database name doesn't already exist with incompatible schema

### CORS Errors
- Ensure the API CORS policy in `Program.cs` matches your Blazor app URL
- Check that both apps are running on the expected ports

### Certificate Generation Errors
- Ensure the `Certificates` folder exists or will be created
- Check file system permissions

### Port Conflicts
- If ports 5000, 5001, 7000, or 7001 are in use, update `launchSettings.json` in both projects

## Next Steps

1. **Integrate NIMC API**: Replace mock NIN verification with actual NIMC API calls
2. **Add SMS/Email Service**: Implement OTP delivery via SMS/Email
3. **Configure Production Settings**: Update JWT secrets, connection strings, and CORS policies
4. **Add Authentication Middleware**: Implement proper JWT validation in Blazor
5. **Deploy**: Set up production hosting and database

## Development Notes

- The database is created automatically on first run using `EnsureCreated()`
- For production, use Entity Framework migrations instead
- JWT tokens are simplified - implement proper JWT library in production
- OTP storage is in-memory - use Redis or database in production
