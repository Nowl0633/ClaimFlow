using ClaimFlow.Data;
using ClaimFlow.Models;

namespace ClaimFlow.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _db;

        public AuditService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string action, string entityType, Guid? entityId = null, Guid? customerId = null)
        {
            // just shove a row in, callers don't need anything back
            var entry = new AuditLog
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OccurredAt = DateTime.UtcNow
            };

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync();
        }
    }
}
