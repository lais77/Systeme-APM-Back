using APM.API.DTOs.Actions;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.API.Controllers
{
    [ApiController]
    [Authorize]
    public class ActionsController : ControllerBase
    {
        private readonly ActionService _actionService;

        public ActionsController(ActionService actionService)
        {
            _actionService = actionService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("api/plans/{planId}/actions")]
        public async Task<IActionResult> GetByPlan(int planId)
        {
            var actions = await _actionService.GetActionsByPlanAsync(planId);
            return Ok(actions);
        }

        [HttpGet("api/actions/mes-actions")]
        public async Task<IActionResult> GetMyActions()
        {
            var actions = await _actionService.GetMyActionsAsync(GetCurrentUserId());
            return Ok(actions);
        }

        [HttpGet("api/actions/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var action = await _actionService.GetActionByIdAsync(id);
            if (action == null) return NotFound();
            return Ok(action);
        }

        [HttpPost("api/plans/{planId}/actions")]
        public async Task<IActionResult> Create(int planId, [FromBody] CreateActionDto dto)
        {
            try
            {
                var action = await _actionService.CreateActionAsync(planId, dto, GetCurrentUserId());
                return CreatedAtAction(nameof(GetById), new { id = action.Id }, action);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("api/actions/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateActionDto dto)
        {
            try
            {
                var action = await _actionService.UpdateActionAsync(id, dto, GetCurrentUserId());
                if (action == null) return NotFound();
                return Ok(action);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpDelete("api/actions/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _actionService.DeleteActionAsync(id, GetCurrentUserId());
                if (!result) return NotFound();
                return Ok(new { message = "Action supprimée." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpPost("api/actions/{id}/demarrer")]
        public async Task<IActionResult> Demarrer(int id)
        {
            var action = await _actionService.DemarrerActionAsync(id, GetCurrentUserId());
            if (action == null) return BadRequest("Action introuvable ou statut invalide.");
            return Ok(action);
        }

        [HttpPost("api/actions/{id}/submit")]
        public async Task<IActionResult> Submit(int id, [FromBody] SubmitActionDto dto)
        {
            var action = await _actionService.SubmitActionAsync(id, dto, GetCurrentUserId());
            if (action == null) return BadRequest("Action introuvable ou statut invalide.");
            return Ok(action);
        }

        [HttpPost("api/actions/{id}/valider")]
        public async Task<IActionResult> Validate(int id, [FromBody] ValidateActionDto dto)
        {
            try
            {
                var action = await _actionService.ValidateActionAsync(id, dto, GetCurrentUserId());
                if (action == null) return BadRequest("Action introuvable ou statut invalide.");
                return Ok(action);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpPost("api/actions/{id}/rejeter")]
        public async Task<IActionResult> Reject(int id, [FromBody] ValidateActionDto dto)
        {
            try
            {
                dto.IsApproved = false;
                var action = await _actionService.ValidateActionAsync(id, dto, GetCurrentUserId());
                if (action == null) return BadRequest("Action introuvable ou statut invalide.");
                return Ok(action);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpPost("api/actions/{id}/evaluer")]
        public async Task<IActionResult> Evaluate(int id, [FromBody] EvaluateActionDto dto)
        {
            try
            {
                var action = await _actionService.EvaluateActionAsync(id, dto, GetCurrentUserId());
                if (action == null) return BadRequest("Action introuvable ou statut invalide.");
                return Ok(action);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }
    }
}