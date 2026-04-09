using APM.API.DTOs.Comments;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.API.Controllers
{
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("api/actions/{actionItemId}/comments")]
        public async Task<IActionResult> GetByAction(int actionItemId)
        {
            var comments = await _commentService.GetByActionAsync(actionItemId);
            return Ok(comments);
        }

        [HttpPost("api/actions/{actionItemId}/comments")]
        public async Task<IActionResult> Add(int actionItemId, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var comment = await _commentService.AddCommentAsync(actionItemId, GetCurrentUserId(), dto);
                return Ok(comment);
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

        [HttpDelete("api/comments/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _commentService.DeleteCommentAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Commentaire supprimé." });
        }
    }
}