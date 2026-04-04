using APM.API.Data;
using APM.API.Entities;

namespace APM.API.Services
{
    public class ActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? userId, string action, string entityType, int? entityId = null, string? details = null)
        {
            var log = new ActivityLog
            {
                UserId = userId ?? 0,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}