using APM.API.Data;
using APM.API.DTOs.Comments;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class CommentService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;

        public CommentService(AppDbContext context, NotificationService notificationService, EmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<List<CommentDto>> GetByActionAsync(int actionItemId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.ActionItemId == actionItemId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    ActionItemId = c.ActionItemId,
                    AuthorId = c.AuthorId,
                    AuthorName = c.Author.FullName,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CommentDto> AddCommentAsync(int actionItemId, int authorId, CreateCommentDto dto)
        {
            var action = await _context.ActionItems
                .Include(a => a.ActionPlan)
                .Include(a => a.Responsible)
                .ThenInclude(r => r!.Manager)
                .FirstOrDefaultAsync(a => a.Id == actionItemId);

            if (action == null)
                throw new InvalidOperationException("Action introuvable.");

            if (action.ActionPlan.Status == "Closed")
                throw new InvalidOperationException("Impossible d'ajouter un commentaire sur un plan clôturé.");
            if (!await CanAccessActionAsync(action, authorId))
                throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à commenter cette action.");

            var comment = new Comment
            {
                ActionItemId = actionItemId,
                AuthorId = authorId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FindAsync(authorId);
            var recipients = new HashSet<int> { action.ResponsibleId, action.ActionPlan.PilotId };
            recipients.Remove(authorId);

            foreach (var recipientId in recipients)
            {
                await _notificationService.CreateInAppAsync(
                    recipientId,
                    "Nouveau commentaire",
                    $"Un commentaire a été ajouté sur l'action \"{action.Theme}\".",
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
                    $"APM — Nouveau commentaire ({action.Theme})",
                    $"<p>Un nouveau commentaire a été ajouté sur l'action <b>{action.Theme}</b>.</p>");
            }

            return new CommentDto
            {
                Id = comment.Id,
                ActionItemId = comment.ActionItemId,
                AuthorId = comment.AuthorId,
                AuthorName = author?.FullName ?? string.Empty,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
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