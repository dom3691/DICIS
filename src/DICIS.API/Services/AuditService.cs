using DICIS.Core.Data;
using DICIS.Core.Entities;
using DICIS.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DICIS.API.Services;

public class AuditService : IAuditService
{
    private readonly DicisDbContext _context;

    public AuditService(DicisDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(string action, string entityType, int? applicationId = null, 
        int? certificateId = null, string? description = null, int? userId = null, 
        string? userRole = null, string? ipAddress = null, string? oldValues = null, 
        string? newValues = null)
    {
        var auditLog = new AuditLog
        {
            ApplicationId = applicationId,
            CertificateId = certificateId,
            Action = action,
            EntityType = entityType,
            Description = description,
            UserId = userId,
            UserRole = userRole,
            IPAddress = ipAddress,
            OldValues = oldValues,
            NewValues = newValues,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(int? applicationId = null, 
        int? certificateId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();
        
        if (applicationId.HasValue)
        {
            query = query.Where(a => a.ApplicationId == applicationId);
        }
        
        if (certificateId.HasValue)
        {
            query = query.Where(a => a.CertificateId == certificateId);
        }
        
        if (fromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= fromDate.Value);
        }
        
        if (toDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= toDate.Value);
        }
        
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }
}
