using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APM.API.Controllers
{
    [ApiController]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly AttachmentService _attachmentService;
        private readonly IWebHostEnvironment _env;

        public AttachmentsController(AttachmentService attachmentService, IWebHostEnvironment env)
        {
            _attachmentService = attachmentService;
            _env = env;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("api/actions/{actionItemId}/attachments")]
        public async Task<IActionResult> GetByAction(int actionItemId)
        {
            var attachments = await _attachmentService.GetByActionAsync(actionItemId);
            return Ok(attachments);
        }

        [HttpPost("api/actions/{actionItemId}/attachments")]
        public async Task<IActionResult> Upload(int actionItemId, IFormFile file, [FromForm] string? description)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Fichier invalide.");

            var attachment = await _attachmentService.UploadAsync(actionItemId, GetCurrentUserId(), file, description);
            return Ok(attachment);
        }

        [HttpDelete("api/attachments/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _attachmentService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Fichier supprimé." });
        }

        [HttpGet("api/attachments/{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var attachment = await _attachmentService.GetByIdAsync(id);
            if (attachment == null) return NotFound();

            var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", attachment.FileName);
        }
    }
}