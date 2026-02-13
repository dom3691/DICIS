using DICIS.Core.Entities;

namespace DICIS.Core.Services;

public interface IAuditService
{
    Task LogActionAsync(string action, string entityType, int? applicationId = null, int? certificateId = null, 
        string? description = null, int? userId = null, string? userRole = null, string? ipAddress = null,
        string? oldValues = null, string? newValues = null);
    Task<List<AuditLog>> GetAuditLogsAsync(int? applicationId = null, int? certificateId = null, DateTime? fromDate = null, DateTime? toDate = null);
}
