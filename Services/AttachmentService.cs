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

        public AttachmentService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
    }
}