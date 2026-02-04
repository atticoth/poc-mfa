using PocMfa.Domain;
using PocMfa.Infrastructure;

namespace PocMfa.Application;

public interface IAuditLogService
{
    Task LogAsync(string userId, string action, string ipAddress, string? metadata = null);
}

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string userId, string action, string ipAddress, string? metadata = null)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = metadata
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
