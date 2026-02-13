using DICIS.Core.DTOs;

namespace DICIS.Core.Services;

public interface ICertificateService
{
    Task<CertificateDTO> GenerateCertificateAsync(int applicationId);
    Task<CertificateVerifyResponse> VerifyCertificateAsync(string certificateId);
    Task<byte[]> GetCertificatePDFAsync(string certificateId);
    Task<bool> RevokeCertificateAsync(string certificateId, string reason, int adminUserId);
}
