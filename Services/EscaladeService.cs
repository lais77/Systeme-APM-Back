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

            // Statuts qui sont en attente de réalisation par le responsable (non clôturés, non annulés, non soumis)
            var statusEnCours = new[] { "Created", "P", "InProgress", "Assigned" };

            var actionsEnCours = await _context.ActionItems
                .Include(a => a.Responsible)
                    .ThenInclude(u => u!.Manager)
                .Include(a => a.ActionPlan)
                    .ThenInclude(p => p.Pilot)
                .Where(a => statusEnCours.Contains(a.Status))
                .ToListAsync();

            foreach (var action in actionsEnCours)
            {
                var diff = (today - action.Deadline.Date).Days;

                if (diff == -1) await SendRappelAsync(action);
                else if (diff == 1) await SendEscaladeN1Async(action);
                else if (diff == 2) await SendEscaladeN2Async(action);
                else if (diff >= 3) await SendEscaladeN3Async(action);
            }

            // Statuts en attente de validation par le pilote
            var statusAValider = new[] { "D", "UnderReview", "Validated" };
            
            var actionsAValider = await _context.ActionItems
                .Include(a => a.ActionPlan)
                    .ThenInclude(p => p.Pilot)
                .Where(a => statusAValider.Contains(a.Status))
                .ToListAsync();

            var actionsParPilote = actionsAValider
                .Where(a => a.ActionPlan?.Pilot != null)
                .GroupBy(a => a.ActionPlan.Pilot);

            foreach (var group in actionsParPilote)
            {
                var pilot = group.Key;
                var count = group.Count();
                var subject = $"APM — Actions en attente de validation";
                var body = $"<h2>Vérification de l'efficacité</h2><p>Bonjour {pilot.FullName},</p><p>Vous avez <b>{count}</b> action(s) en attente de validation de l'efficacité.</p><p>Merci de vous connecter à l'APM pour les évaluer.</p>";
                await _emailService.SendEmailAsync(pilot.Email, pilot.FullName, subject, body);
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