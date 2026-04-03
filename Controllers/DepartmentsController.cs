using APM.API.DTOs.Admin;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : ControllerBase
    {
        private readonly DepartmentService _deptService;

        public DepartmentsController(DepartmentService deptService)
        {
            _deptService = deptService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _deptService.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto) =>
            Ok(await _deptService.CreateAsync(dto));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
        {
            var result = await _deptService.UpdateAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _deptService.DeleteAsync(id);
            return result ? Ok(new { message = "Département supprimé." }) : NotFound();
        }
    }
}