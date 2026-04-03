using APM.API.DTOs.Admin;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TeamsController : ControllerBase
    {
        private readonly TeamService _teamService;

        public TeamsController(TeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _teamService.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamDto dto) =>
            Ok(await _teamService.CreateAsync(dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _teamService.DeleteAsync(id);
            return result ? Ok(new { message = "Équipe supprimée." }) : NotFound();
        }
    }
}