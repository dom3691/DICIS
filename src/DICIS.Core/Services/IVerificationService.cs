using DICIS.Core.DTOs;

namespace DICIS.Core.Services;

public interface IVerificationService
{
    Task<VerificationResponse> VerifyApplicationAsync(int applicationId);
    Task<bool> VerifyParentageAsync(string applicantNIN, string? fatherNIN, string? motherNIN, string state);
    Task<bool> CheckDuplicateCertificateAsync(string nin, string state);
    Task<decimal> CalculateRiskScoreAsync(int applicationId);
    Task<decimal> CalculateConfidenceScoreAsync(int applicationId);
}
