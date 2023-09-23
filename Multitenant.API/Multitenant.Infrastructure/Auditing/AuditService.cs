namespace Multitenant.Infrastructure.Auditing
{
    using Microsoft.EntityFrameworkCore;

    using Mapster;

    using Multitenant.Models.Auditing;
    using Multitenant.Application.Interfaces.Auditing;
    using Multitenant.Infrastructure.Services.Tenant.Context;

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context) => _context = context;

        public async Task<List<AuditDto>> GetUserTrailsAsync(Guid userId)
        {
            var trails = await _context.AuditTrails
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.DateTime)
                .Take(250)
                .ToListAsync();

            return trails.Adapt<List<AuditDto>>();
        }
    }
}