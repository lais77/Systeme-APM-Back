using APM.API.Data;
using APM.API.DTOs.Comments;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class CommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
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
    }
}