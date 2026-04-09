using APM.API.Data;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class EscaladeService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly NotificationService _notificationService;

        public EscaladeService(AppDbContext context, EmailService emailService, NotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task RunDailyCheckAsync()
        {
            var today = DateTime.UtcNow.Date;

            var actions = await _context.ActionItems
                .Include(a => a.Responsible)
                    .ThenInclude(u => u!.Manager)
                .Include(a => a.ActionPlan)
                    .ThenInclude(p => p.Pilot)
                .Where(a =>
                    a.Status != "C" &&
                    a.Status != "Closed" &&
                    a.Status != "Clôturé" &&
                    a.Status != "Annulé")
                .ToListAsync();

            foreach (var action in actions)
            {
                var diff = (today - action.Deadline.Date).Days;

                if (diff == -1) await SendRappelAsync(action);
                else if (diff == 1) await SendEscaladeN1Async(action);
                else if (diff == 2) await SendEscaladeN2Async(action);
                else if (diff >= 3) await SendEscaladeN3Async(action);
            }
        }

        private async Task SendRappelAsync(ActionItem action)
        {
            if (action.Responsible == null) return;
            var subject = $"APM — Rappel : action expire demain";
            var body = $"<h2>Rappel échéance</h2><p>Votre action <b>{action.Theme}</b> expire demain le {action.Deadline:dd/MM/yyyy}.</p>";
            await _emailService.SendEmailAsync(action.Responsible.Email, action.Responsible.FullName, subject, body);
        }

        private async Task SendEscaladeN1Async(ActionItem action)
        {
            if (action.Responsible == null) return;
            var subject = $"APM — Retard 24h : {action.Theme}";
            var body = $"<h2>Retard 24h</h2><p>L'action <b>{action.Theme}</b> est en retard de 24h.</p>";
            await _emailService.SendEmailAsync(action.Responsible.Email, action.Responsible.FullName, subject, body);

            if (action.ActionPlan?.Pilot != null)
                await _emailService.SendEmailAsync(action.ActionPlan.Pilot.Email, action.ActionPlan.Pilot.FullName, subject, body);
        }

        private async Task SendEscaladeN2Async(ActionItem action)
        {
            await SendEscaladeN1Async(action);
            if (action.Responsible?.Manager != null)
            {
                var subject = $"APM — Retard 48h : {action.Theme}";
                var body = $"<h2>Retard 48h</h2><p>L'action <b>{action.Theme}</b> est en retard de 48h.</p>";
                await _emailService.SendEmailAsync(action.Responsible.Manager.Email, action.Responsible.Manager.FullName, subject, body);
            }
        }

        private async Task SendEscaladeN3Async(ActionItem action)
        {
            await SendEscaladeN2Async(action);
            var direction = await _context.Users.FirstOrDefaultAsync(u => u.Role == "AUDITEUR");
            if (direction != null)
            {
                var subject = $"APM — CRITIQUE Retard 72h+ : {action.Theme}";
                var body = $"<h2>Retard critique</h2><p>L'action <b>{action.Theme}</b> est en retard de 72h ou plus.</p>";
                await _emailService.SendEmailAsync(direction.Email, direction.FullName, subject, body);
            }
        }
    }
}