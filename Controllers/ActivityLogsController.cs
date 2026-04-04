using APM.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class ActivityLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ActivityLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.Id,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.Details,
                    l.CreatedAt,
                    UserName = l.User != null ? l.User.FullName : "Système"
                })
                .Take(500)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var logs = await _context.ActivityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.Id,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.Details,
                    l.CreatedAt
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}