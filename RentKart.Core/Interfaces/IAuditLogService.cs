using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IAuditLogService
{
    Task LogActionAsync(string? userId, string action, string entityName, string entityId, string? description = null, string? ipAddress = null);
    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 50);
}
