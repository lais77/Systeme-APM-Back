using APM.API.Data;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public NotificationService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task CreateInAppAsync(int userId, string title, string message, int? actionItemId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetMyNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            notifications.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
        }

        public async Task SendActionAssignedAsync(ActionItem action)
        {
            var subject = $"APM — Nouvelle action assignée : {action.Theme}";
            var body = $@"
                <h2>Nouvelle action assignée</h2>
                <p>Bonjour {action.Responsible?.FullName},</p>
                <p>Une nouvelle action vous a été assignée :</p>
                <ul>
                    <li><b>Thème :</b> {action.Theme}</li>
                    <li><b>Délai :</b> {action.Deadline:dd/MM/yyyy}</li>
                    <li><b>Criticité :</b> {action.Criticity}</li>
                </ul>
                <p>Connectez-vous à APM pour consulter les détails.</p>";

            if (action.Responsible != null)
            {
                await _emailService.SendEmailAsync(
                    action.Responsible.Email,
                    action.Responsible.FullName,
                    subject, body);

                await CreateInAppAsync(
                    action.ResponsibleId,
                    "Nouvelle action assignée",
                    $"Action : {action.Theme}",
                    action.Id);
            }
        }
    }
}