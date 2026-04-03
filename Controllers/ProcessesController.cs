using APM.API.DTOs.Admin;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProcessesController : ControllerBase
    {
        private readonly ProcessService _processService;

        public ProcessesController(ProcessService processService)
        {
            _processService = processService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _processService.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProcessDto dto) =>
            Ok(await _processService.CreateAsync(dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _processService.DeleteAsync(id);
            return result ? Ok(new { message = "Processus supprimé." }) : NotFound();
        }
    }
}