using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/support/tickets")]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private static readonly List<SupportTicketDto> Tickets = new();
        private static int _nextId = 1;

        [HttpGet("my")]
        public IActionResult GetMyTickets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var mine = Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            return Ok(mine);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateSupportTicketDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ticket = new SupportTicketDto
            {
                Id = _nextId++,
                UserId = userId,
                Category = dto.Category,
                Priority = dto.Priority,
                Status = "OPEN",
                Message = dto.Message,
                PageUrl = dto.PageUrl,
                FileName = dto.FileName,
                CreatedAt = DateTime.UtcNow.ToString("o")
            };

            Tickets.Add(ticket);
            return Ok(ticket);
        }

        public class CreateSupportTicketDto
        {
            public string Category { get; set; } = string.Empty;
            public string Priority { get; set; } = "MEDIUM";
            public string Message { get; set; } = string.Empty;
            public string PageUrl { get; set; } = string.Empty;
            public string? FileName { get; set; }
        }

        public class SupportTicketDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Priority { get; set; } = string.Empty;
            public string Status { get; set; } = "OPEN";
            public string Message { get; set; } = string.Empty;
            public string PageUrl { get; set; } = string.Empty;
            public string? FileName { get; set; }
            public string CreatedAt { get; set; } = string.Empty;
        }
    }
}
