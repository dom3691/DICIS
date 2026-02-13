# Digital Indigene Certificate Issuance System (DICIS)

A comprehensive digital platform for issuing and verifying indigene certificates in Nigeria, enabling same-day, officer-less, digitally verifiable certificate issuance.

## Features

- **Automated Verification**: NIN-based identity validation with automated risk scoring
- **Fast Processing**: 2-5 minute certificate issuance for standard cases
- **Digital Certificates**: PDF certificates with embedded QR codes and digital signatures
- **Public Verification**: QR code and certificate ID-based verification portal
- **Audit Trail**: Immutable audit logs for all actions
- **Admin Dashboard**: Analytics and management tools for administrators
- **Fraud Detection**: Automated risk scoring and fraud reporting system

## Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API (C#)
- **Frontend**: Blazor WebAssembly
- **Database**: SQL Server (Entity Framework Core)
- **Authentication**: JWT Bearer Tokens
- **PDF Generation**: iTextSharp
- **QR Code Generation**: QRCoder

## Project Structure

```
DICIS/
├── src/
│   ├── DICIS.API/          # Backend Web API
│   ├── DICIS.Blazor/       # Frontend Blazor WebAssembly
│   └── DICIS.Core/         # Shared models and DTOs
└── DICIS.sln              # Solution file
```

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

## Setup Instructions

### 1. Clone and Restore

```bash
# Restore NuGet packages
dotnet restore
```

### 2. Database Setup

Update the connection string in `src/DICIS.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DICIS;Trusted_Connection=True;"
  }
}
```

The database will be created automatically on first run.

### 3. Configure JWT Secret

Update the JWT secret in `src/DICIS.API/appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-change-in-production-min-32-chars-long-for-security"
  }
}
```

### 4. Run the Application

#### Backend API

```bash
cd src/DICIS.API
dotnet run
```

The API will be available at `https://localhost:7000` (or `http://localhost:5000`)

#### Frontend Blazor

```bash
cd src/DICIS.Blazor
dotnet run
```

The Blazor app will be available at `https://localhost:7001` (or `http://localhost:5001`)

**Note**: Update the API base URL in `src/DICIS.Blazor/Program.cs` to match your API URL.

## API Endpoints

### Authentication
- `POST /api/auth/nin-verify` - Verify NIN
- `POST /api/auth/otp-verify` - Verify OTP and login

### Applications
- `POST /api/applications` - Create application
- `GET /api/applications` - Get user's applications
- `GET /api/applications/{id}` - Get application by ID
- `PUT /api/applications/{id}` - Update application
- `POST /api/applications/{id}/submit` - Submit application

### Verification
- `POST /api/verification/run` - Run verification
- `GET /api/verification/status/{applicationId}` - Get verification status

### Certificates
- `POST /api/certificate/generate` - Generate certificate
- `GET /api/certificate/{id}` - Get certificate
- `GET /api/certificate/verify/{certificateId}` - Verify certificate (public)
- `GET /api/certificate/download/{certificateId}` - Download PDF
- `POST /api/certificate/revoke` - Revoke certificate (admin)

### Admin
- `GET /api/admin/analytics` - Get analytics
- `POST /api/admin/fraud-report` - Submit fraud report

## Usage

### For Citizens

1. **Login**: Navigate to `/login` and enter your NIN
2. **Verify OTP**: Enter the OTP sent to your registered phone/email
3. **Apply**: Fill out the application form at `/apply`
4. **Submit**: Submit your application for automated verification
5. **Download**: Once approved, download your certificate

### For Verifiers

1. Navigate to `/verify`
2. Enter the Certificate ID or scan the QR code
3. View the verification results

### For Administrators

1. Login with admin credentials
2. Access the admin dashboard at `/admin`
3. View analytics and manage applications

## Key Modules

### 1. Authentication & Identity Validation
- NIN-based login
- OTP verification
- JWT token generation

### 2. Application Submission
- State and LGA selection
- Parentage linkage
- Document upload support
- Draft saving

### 3. Automated Verification Engine
- NIN validation
- Parent-child linkage verification
- Duplicate detection
- LGA consistency checks

### 4. Risk Scoring Engine
- Confidence scoring
- Auto-approval threshold (80%)
- Exception routing for low scores

### 5. Certificate Generation
- Unique certificate ID
- PDF generation with digital signature
- QR code embedding
- Hash-based integrity verification

### 6. Public Verification Portal
- QR code scanning
- Certificate ID lookup
- Status verification
- Revocation checking

### 7. Audit & Revocation
- Immutable audit logs
- Fraud reporting
- Certificate revocation
- Admin notifications

## Development Notes

### NIMC Integration

The current implementation uses mock data for NIMC API calls. In production, integrate with the actual NIMC API:

- Update `AuthService.VerifyNINAsync()` to call NIMC API
- Update `VerificationService.VerifyParentageAsync()` to verify parent-child linkage
- Implement proper OTP delivery via SMS/Email service

### Security Considerations

- Change JWT secret in production
- Use HTTPS in production
- Implement proper CORS policies
- Add rate limiting
- Implement proper logging and monitoring
- Use secure file storage for certificates

### Database Migrations

To create migrations:

```bash
cd src/DICIS.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## License

This project is developed for the Nigerian government's Digital Indigene Certificate Issuance System.

## Support

For issues and questions, please contact the development team.
