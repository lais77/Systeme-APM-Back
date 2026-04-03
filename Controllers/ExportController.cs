using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly ExportService _exportService;

        public ExportController(ExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var bytes = await _exportService.ExportToExcelAsync();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"APM_Export_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}