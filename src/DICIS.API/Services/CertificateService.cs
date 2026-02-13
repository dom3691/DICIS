using DICIS.Core.Data;
using DICIS.Core.DTOs;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace DICIS.API.Services;

public class CertificateService : ICertificateService
{
    private readonly DicisDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IAuditService _auditService;

    public CertificateService(DicisDbContext context, IWebHostEnvironment environment, IAuditService auditService)
    {
        _context = context;
        _environment = environment;
        _auditService = auditService;
    }

    public async Task<CertificateDTO> GenerateCertificateAsync(int applicationId)
    {
        var application = await _context.Applications
            .Include(a => a.User)
            .Include(a => a.Certificate)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
        
        if (application == null)
        {
            throw new Exception("Application not found");
        }
        
        if (application.Status != ApplicationStatus.Approved)
        {
            throw new Exception("Application must be approved before certificate generation");
        }
        
        if (application.Certificate != null)
        {
            return new CertificateDTO
            {
                CertificateId = application.Certificate.CertificateId,
                QRCodeData = application.Certificate.QRCodeData,
                PDFPath = application.Certificate.PDFPath,
                Status = application.Certificate.Status,
                IssuedAt = application.Certificate.IssuedAt,
                ExpiresAt = application.Certificate.ExpiresAt
            };
        }
        
        // Generate unique certificate ID
        var certificateId = GenerateCertificateId(application);
        
        // Generate QR code
        var baseUrl = _environment.IsDevelopment() ? "https://localhost:5001" : "https://dicis.gov.ng";
        var verificationUrl = $"{baseUrl}/verify/{certificateId}";
        var qrCodeData = GenerateQRCode(verificationUrl);
        
        // Generate PDF
        var pdfPath = await GeneratePDFAsync(application, certificateId, qrCodeData);
        
        // Calculate hash
        var hash = CalculateHash(pdfPath);
        
        // Create certificate entity
        var certificate = new Certificate
        {
            ApplicationId = applicationId,
            CertificateId = certificateId,
            QRCodeData = qrCodeData,
            PDFPath = pdfPath,
            Hash = hash,
            Status = CertificateStatus.Active,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(10)
        };
        
        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();
        
        await _auditService.LogActionAsync(
            "CertificateGenerated",
            "Certificate",
            applicationId,
            certificate.Id,
            $"Certificate {certificateId} generated for {application.User.FirstName} {application.User.LastName}"
        );
        
        return new CertificateDTO
        {
            CertificateId = certificate.CertificateId,
            QRCodeData = certificate.QRCodeData,
            PDFPath = certificate.PDFPath,
            Status = certificate.Status,
            IssuedAt = certificate.IssuedAt,
            ExpiresAt = certificate.ExpiresAt
        };
    }

    public async Task<CertificateVerifyResponse> VerifyCertificateAsync(string certificateId)
    {
        var certificate = await _context.Certificates
            .Include(c => c.Application)
            .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId);
        
        if (certificate == null)
        {
            return new CertificateVerifyResponse
            {
                IsValid = false,
                Message = "Certificate not found"
            };
        }
        
        // Verify hash integrity
        var currentHash = CalculateHash(certificate.PDFPath);
        var isTampered = currentHash != certificate.Hash;
        
        if (isTampered)
        {
            return new CertificateVerifyResponse
            {
                IsValid = false,
                Message = "Certificate has been tampered with"
            };
        }
        
        return new CertificateVerifyResponse
        {
            IsValid = certificate.Status == CertificateStatus.Active,
            Name = $"{certificate.Application.User.FirstName} {certificate.Application.User.LastName}",
            State = certificate.Application.State,
            LGA = certificate.Application.LGA,
            Status = certificate.Status.ToString(),
            IssuedAt = certificate.IssuedAt,
            IsRevoked = certificate.Status == CertificateStatus.Revoked,
            RevocationReason = certificate.RevocationReason,
            Message = certificate.Status == CertificateStatus.Active ? "Certificate is valid" : "Certificate is revoked or expired"
        };
    }

    public async Task<byte[]> GetCertificatePDFAsync(string certificateId)
    {
        var certificate = await _context.Certificates
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId);
        
        if (certificate == null)
        {
            throw new Exception("Certificate not found");
        }
        
        var filePath = Path.Combine(_environment.ContentRootPath, "Certificates", certificate.PDFPath);
        
        if (!File.Exists(filePath))
        {
            throw new Exception("Certificate PDF file not found");
        }
        
        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<bool> RevokeCertificateAsync(string certificateId, string reason, int adminUserId)
    {
        var certificate = await _context.Certificates
            .Include(c => c.Application)
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId);
        
        if (certificate == null)
        {
            return false;
        }
        
        certificate.Status = CertificateStatus.Revoked;
        certificate.RevocationReason = reason;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.RevokedBy = adminUserId;
        
        await _context.SaveChangesAsync();
        
        await _auditService.LogActionAsync(
            "CertificateRevoked",
            "Certificate",
            certificate.ApplicationId,
            certificate.Id,
            $"Certificate revoked: {reason}",
            adminUserId,
            "Admin"
        );
        
        return true;
    }

    private string GenerateCertificateId(Application application)
    {
        var prefix = application.State.Substring(0, Math.Min(3, application.State.Length)).ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"{prefix}-{timestamp}-{random}";
    }

    private string GenerateQRCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }

    private async Task<string> GeneratePDFAsync(Application application, string certificateId, string qrCodeBase64)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        
        var certificatesDir = Path.Combine(_environment.ContentRootPath, "Certificates");
        if (!Directory.Exists(certificatesDir))
        {
            Directory.CreateDirectory(certificatesDir);
        }
        
        var fileName = $"{certificateId}.pdf";
        var filePath = Path.Combine(certificatesDir, fileName);
        
        var fullName = $"{application.User.FirstName} {application.User.MiddleName} {application.User.LastName}".Trim();
        var bodyText = $"This is to certify that {fullName} (NIN: {application.User.NIN}) is an indigene of {application.State} State, specifically from {application.LGA} Local Government Area.";
        var dateText = $"Issued on: {DateTime.UtcNow:dd MMMM yyyy}";
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(20);
                        
                        // Title
                        column.Item()
                            .AlignCenter()
                            .Text("CERTIFICATE OF INDIGENESHIP")
                            .FontSize(24)
                            .Bold();
                        
                        // Certificate ID
                        column.Item()
                            .AlignRight()
                            .Text($"Certificate ID: {certificateId}")
                            .FontSize(10);
                        
                        // Body text
                        column.Item()
                            .PaddingVertical(20)
                            .Text(bodyText)
                            .FontSize(12)
                            .Justify();
                        
                        // Date
                        column.Item()
                            .Text(dateText)
                            .FontSize(12);
                        
                        // QR Code
                        if (!string.IsNullOrEmpty(qrCodeBase64))
                        {
                            try
                            {
                                var qrBytes = Convert.FromBase64String(qrCodeBase64);
                                column.Item()
                                    .AlignRight()
                                    .Width(100)
                                    .Height(100)
                                    .Image(qrBytes);
                            }
                            catch
                            {
                                // If QR code fails, continue without it
                            }
                        }
                    });
            });
        });
        
        await Task.Run(() => document.GeneratePdf(filePath));
        
        return fileName;
    }

    private string CalculateHash(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, "Certificates", filePath);
        if (!File.Exists(fullPath))
        {
            return string.Empty;
        }
        
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(fullPath);
        var hashBytes = sha256.ComputeHash(fileStream);
        return Convert.ToHexString(hashBytes);
    }
}
