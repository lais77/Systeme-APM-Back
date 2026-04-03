using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly StatsService _statsService;

        public StatsController(StatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet("global")]
        public async Task<IActionResult> GetGlobal() =>
            Ok(await _statsService.GetGlobalStatsAsync());

        [HttpGet("by-department")]
        public async Task<IActionResult> GetByDepartment() =>
            Ok(await _statsService.GetStatsByDepartmentAsync());

        [HttpGet("by-pilot")]
        public async Task<IActionResult> GetByPilot() =>
            Ok(await _statsService.GetStatsByPilotAsync());

        [HttpGet("monthly/{year}")]
        public async Task<IActionResult> GetMonthly(int year) =>
            Ok(await _statsService.GetMonthlyStatsAsync(year));
    }
}