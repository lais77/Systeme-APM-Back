using APM.API.DTOs.Plans;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/plans")]
    [Authorize]
    public class PlansController : ControllerBase
    {
        private readonly PlanService _planService;

        public PlansController(PlanService planService)
        {
            _planService = planService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _planService.GetAllPlansAsync();
            return Ok(plans);
        }

        [HttpGet("mes-plans")]
        public async Task<IActionResult> GetMyPlans()
        {
            var plans = await _planService.GetMyPlansAsync(GetCurrentUserId());
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plan = await _planService.GetPlanByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlanDto dto)
        {
            var plan = await _planService.CreatePlanAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanDto dto)
        {
            var plan = await _planService.UpdatePlanAsync(id, dto);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        [HttpPatch("{id}/cloturer")]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _planService.ClosePlanAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Plan clôturé avec succès." });
        }
    }
}