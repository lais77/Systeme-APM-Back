using APM.API.Services;
using APM.API.DTOs.Stats;
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

        [HttpPost("by-periode")]
        public async Task<IActionResult> GetByPeriod([FromBody] StatsByPeriodDto? dto) =>
            Ok(await _statsService.GetStatsByPeriodAsync(dto?.startDate, dto?.endDate));

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance() =>
            Ok(await _statsService.GetPerformanceStatsAsync());

        [HttpGet("plans-critiques")]
        public async Task<IActionResult> GetPlansCritiques()
        {
            var result = await _statsService.GetPlansCritiquesAsync();
            return Ok(result);
        }

        [HttpGet("activite-recente")]
        public async Task<IActionResult> GetActiviteRecente()
        {
            // Note: Normalement ActivityLogService devrait gérer ça, mais on peut le faire ici via le contexte si injecté
            // ou via un nouveau service. Pour faire simple et respecter la demande :
            // On suppose que StatsService ou le controller a accès au DbContext (ici StatsController n'a que StatsService).
            // Cependant, la demande montre le code utilisant _context.
            // Je vais donc ajouter la méthode au StatsService pour rester propre, ou modifier le controller.
            // Le user a mis le code dans le controller dans son exemple.
            // Je vais vérifier si StatsController a accès à AppDbContext. 
            // D'après ma lecture précédente, il n'a que StatsService.
            
            // Correction : je vais mettre la logique dans StatsService et l'appeler depuis le controller.
            return Ok(await _statsService.GetActiviteRecenteAsync());
        }
    }
}