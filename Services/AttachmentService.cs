using APM.API.Data;
using APM.API.DTOs.Attachments;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class AttachmentService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;

        public AttachmentService(AppDbContext context, IWebHostEnvironment env, NotificationService notificationService, EmailService emailService)
        {
            _context = context;
            _env = env;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<List<AttachmentDto>> GetByActionAsync(int actionItemId)
        {
            return await _context.Attachments
                .Include(a => a.UploadedBy)
                .Where(a => a.ActionItemId == actionItemId)
                .Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    ActionItemId = a.ActionItemId,
                    FileName = a.FileName,
                    Description = a.Description,
                    FilePath = a.FilePath,
                    UploadedAt = a.UploadedAt,
                    UploadedById = a.UploadedById,
                    UploadedByName = a.UploadedBy.FullName
                })
                .ToListAsync();
        }

        public async Task<AttachmentDto> UploadAsync(int actionItemId, int uploadedById, IFormFile file, string? description)
        {
            var action = await _context.ActionItems
                .Include(a => a.ActionPlan)
                .Include(a => a.Responsible)
                .FirstOrDefaultAsync(a => a.Id == actionItemId);

            if (action == null)
                throw new InvalidOperationException("Action introuvable.");

            if (action.ActionPlan.Status == "Closed")
                throw new InvalidOperationException("Impossible d'ajouter un fichier sur un plan clôturé.");
            if (!await CanAccessActionAsync(action, uploadedById))
                throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à ajouter un fichier sur cette action.");

            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                ActionItemId = actionItemId,
                FileName = file.FileName,
                Description = description,
                FilePath = uniqueFileName,
                UploadedAt = DateTime.UtcNow,
                UploadedById = uploadedById
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            var uploader = await _context.Users.FindAsync(uploadedById);
            var recipients = new HashSet<int> { action.ResponsibleId, action.ActionPlan.PilotId };
            recipients.Remove(uploadedById);

            foreach (var recipientId in recipients)
            {
                await _notificationService.CreateInAppAsync(
                    recipientId,
                    "Nouveau fichier annexe",
                    $"Un fichier a été ajouté sur l'action \"{action.Theme}\".",
                    action.Id);
            }

            var users = await _context.Users
                .Where(u => recipients.Contains(u.Id))
                .Select(u => new { u.Email, u.FullName })
                .ToListAsync();
            foreach (var user in users)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    user.FullName,
                    $"APM — Nouveau fichier annexe ({action.Theme})",
                    $"<p>Un nouveau fichier annexe a été ajouté sur l'action <b>{action.Theme}</b>.</p>");
            }

            return new AttachmentDto
            {
                Id = attachment.Id,
                ActionItemId = attachment.ActionItemId,
                FileName = attachment.FileName,
                Description = attachment.Description,
                FilePath = attachment.FilePath,
                UploadedAt = attachment.UploadedAt,
                UploadedById = attachment.UploadedById,
                UploadedByName = uploader?.FullName ?? string.Empty
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null) return false;

            var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.FilePath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Attachment?> GetByIdAsync(int id)
        {
            return await _context.Attachments.FindAsync(id);
        }

        private async Task<bool> CanAccessActionAsync(ActionItem action, int userId)
        {
            if (action.ResponsibleId == userId || action.ActionPlan.PilotId == userId) return true;

            var role = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }
    }
}