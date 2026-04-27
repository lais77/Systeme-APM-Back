using APM.API.Data;
using APM.API.DTOs.Actions;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace APM.API.Services
{
    public class ActionService
    {
        private readonly AppDbContext _context;
        private readonly PlanService _planService;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;

        public ActionService(AppDbContext context, PlanService planService, NotificationService notificationService, EmailService emailService)
        {
            _context = context;
            _planService = planService;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<List<ActionDto>> GetActionsByPlanAsync(int planId)
        {
            return await _context.ActionItems
                .Include(a => a.Responsible)
                .Where(a => a.ActionPlanId == planId)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<List<ActionDto>> GetMyActionsAsync(int userId)
        {
            return await _context.ActionItems
                .Include(a => a.Responsible)
                .Where(a => a.ResponsibleId == userId)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<ActionDto?> GetActionByIdAsync(int id)
        {
            var action = await _context.ActionItems
                .Include(a => a.Responsible)
                .Include(a => a.Comments)
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);

            return action == null ? null : MapToDto(action);
        }

        public async Task<ActionDto> CreateActionAsync(int planId, CreateActionDto dto, int actingUserId)
        {
            if (await IsPlanClosedAsync(planId))
                throw new InvalidOperationException("Impossible d'ajouter une action à un plan clôturé.");
            if (!await IsPilotOrAdminForPlanAsync(planId, actingUserId))
                throw new UnauthorizedAccessException("Seul le pilote du plan peut ajouter une action.");

            var action = new ActionItem
            {
                Theme = dto.Theme,
                AnomalyDescription = dto.AnomalyDescription,
                ActionDescription = dto.ActionDescription,
                Type = dto.Type,
                Criticity = dto.Criticity,
                Cause = dto.Cause,
                Deadline = dto.Deadline,
                ResponsibleId = dto.ResponsibleId,
                ActionPlanId = planId,
                Status = "P",
                CreatedAt = DateTime.UtcNow
            };

            _context.ActionItems.Add(action);
            await _context.SaveChangesAsync();
            await _context.Entry(action).Reference(a => a.Responsible).LoadAsync();
            await _notificationService.SendActionAssignedAsync(action);
            await _planService.UpdateProgressAsync(planId);

            return (await GetActionByIdAsync(action.Id))!;
        }

        public async Task<ActionDto?> UpdateActionAsync(int id, UpdateActionDto dto, int actingUserId)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null) return null;
            if (await IsPlanClosedAsync(action.ActionPlanId)) return null;

            var previousResponsibleId = action.ResponsibleId;
            var modifications = new List<string>();

            if (dto.Theme != null && dto.Theme != action.Theme)
            {
                action.Theme = dto.Theme;
                modifications.Add("theme");
            }
            if (dto.ActionDescription != null && dto.ActionDescription != action.ActionDescription)
            {
                action.ActionDescription = dto.ActionDescription;
                modifications.Add("description");
            }
            if (dto.Type != null && dto.Type != action.Type)
            {
                action.Type = dto.Type;
                modifications.Add("type");
            }
            if (dto.Criticity != null && dto.Criticity != action.Criticity)
            {
                action.Criticity = dto.Criticity;
                modifications.Add("criticite");
            }
            if (dto.Cause != null && dto.Cause != action.Cause)
            {
                action.Cause = dto.Cause;
                modifications.Add("cause");
            }
            if (dto.Deadline.HasValue && dto.Deadline.Value != action.Deadline)
            {
                action.Deadline = dto.Deadline.Value;
                modifications.Add("echeance");
            }
            if (dto.ResponsibleId.HasValue && dto.ResponsibleId.Value != action.ResponsibleId)
            {
                action.ResponsibleId = dto.ResponsibleId.Value;
                modifications.Add("responsable");
            }
            if (dto.ProgressPercentage.HasValue && dto.ProgressPercentage.Value != action.ProgressPercentage)
            {
                action.ProgressPercentage = dto.ProgressPercentage.Value;
                modifications.Add("avancement");
            }

            action.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (action.ResponsibleId != previousResponsibleId)
            {
                await _context.Entry(action).Reference(a => a.Responsible).LoadAsync();
                await _notificationService.SendActionAssignedAsync(action);
            }

            if (modifications.Count > 0)
                await NotifyActionUpdatedAsync(action, actingUserId, modifications);

            await _planService.UpdateProgressAsync(action.ActionPlanId);

            return await GetActionByIdAsync(id);
        }

        public async Task<bool> DeleteActionAsync(int id, int actingUserId)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null) return false;
            if (await IsPlanClosedAsync(action.ActionPlanId)) return false;
            if (!await IsPilotOrAdminForPlanAsync(action.ActionPlanId, actingUserId))
                throw new UnauthorizedAccessException("Seul le pilote du plan peut supprimer l'action.");

            var planId = action.ActionPlanId;
            _context.ActionItems.Remove(action);
            await _context.SaveChangesAsync();
            await _planService.UpdateProgressAsync(planId);
            return true;
        }

        public async Task<ActionDto?> DemarrerActionAsync(int id, int actingUserId)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || (action.Status != "Created" && action.Status != "P")) return null;
            if (await IsPlanClosedAsync(action.ActionPlanId)) return null;
            if (!await IsResponsibleOrAdminForActionAsync(action, actingUserId)) return null;

            action.Status = "InProgress";
            action.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetActionByIdAsync(id);
        }

        public async Task<ActionDto?> SubmitActionAsync(int id, SubmitActionDto dto, int actingUserId)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || action.Status != "InProgress") return null;
            if (await IsPlanClosedAsync(action.ActionPlanId)) return null;
            if (!await IsResponsibleOrAdminForActionAsync(action, actingUserId)) return null;

            action.Status = "D";
            action.RealizationMethod = dto.RealizationMethod;
            action.RealizationDate = dto.RealizationDate == default ? DateTime.UtcNow : dto.RealizationDate;
            action.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetActionByIdAsync(id);
        }

        public async Task<ActionDto?> ValidateActionAsync(int id, ValidateActionDto dto, int actingUserId)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || (action.Status != "UnderReview" && action.Status != "D")) return null;
            if (await IsPlanClosedAsync(action.ActionPlanId)) return null;
            if (!await IsPilotOrAdminForPlanAsync(action.ActionPlanId, actingUserId))
                throw new UnauthorizedAccessException("Seul le pilote du plan peut valider l'action.");

            action.Status = dto.IsApproved ? "C" : "P";
            if (!dto.IsApproved && dto.Comment != null)
                action.EffectivenessComment = dto.Comment;

            action.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetActionByIdAsync(id);
        }

        public async Task<ActionDto?> EvaluateActionAsync(int id, EvaluateActionDto dto, int actingUserId)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || (action.Status != "Validated" && action.Status != "D" && action.Status != "UnderReview")) return null;
            if (await IsPlanClosedAsync(action.ActionPlanId)) return null;
            if (!await IsPilotOrAdminForPlanAsync(action.ActionPlanId, actingUserId))
                throw new UnauthorizedAccessException("Seul le pilote du plan peut évaluer l'action.");

            action.Effectiveness = dto.Effectiveness;
            action.EffectivenessComment = dto.Comment;
            action.StarRating = dto.StarRating;
            action.ModifiedAt = DateTime.UtcNow;

            if (dto.Effectiveness == "Ineffective" && dto.ReplacementAction != null)
            {
                action.Status = "C";
                var replacement = new ActionItem
                {
                    Theme = dto.ReplacementAction.Theme,
                    ActionDescription = dto.ReplacementAction.ActionDescription,
                    Type = dto.ReplacementAction.Type,
                    Criticity = dto.ReplacementAction.Criticity,
                    Cause = dto.ReplacementAction.Cause,
                    Deadline = dto.ReplacementAction.Deadline,
                    ResponsibleId = dto.ReplacementAction.ResponsibleId,
                    ActionPlanId = action.ActionPlanId,
                    ParentActionId = action.Id,
                    Status = "P",
                    CreatedAt = DateTime.UtcNow
                };
                _context.ActionItems.Add(replacement);
            }
            else
            {
                action.Status = "C";
                action.ProgressPercentage = 100;
            }

            await _context.SaveChangesAsync();
            await _planService.UpdateProgressAsync(action.ActionPlanId);
            return await GetActionByIdAsync(id);
        }

        private async Task<bool> IsPlanClosedAsync(int planId)
        {
            var planStatus = await _context.ActionPlans
                .Where(p => p.Id == planId)
                .Select(p => p.Status)
                .FirstOrDefaultAsync();

            return string.Equals(planStatus, "Closed", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> IsPilotOrAdminForPlanAsync(int planId, int userId)
        {
            var plan = await _context.ActionPlans
                .Where(p => p.Id == planId)
                .Select(p => new { p.PilotId })
                .FirstOrDefaultAsync();
            if (plan == null) return false;

            if (plan.PilotId == userId) return true;
            var role = await _context.Users.Where(u => u.Id == userId).Select(u => u.Role).FirstOrDefaultAsync();
            return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> IsResponsibleOrAdminForActionAsync(ActionItem action, int userId)
        {
            if (action.ResponsibleId == userId) return true;
            var role = await _context.Users.Where(u => u.Id == userId).Select(u => u.Role).FirstOrDefaultAsync();
            return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }

        private static ActionDto MapToDto(ActionItem a) => new ActionDto
        {
            Id = a.Id,
            Theme = a.Theme,
            AnomalyDescription = a.AnomalyDescription,
            ActionDescription = a.ActionDescription,
            Type = a.Type,
            Criticity = a.Criticity,
            Cause = a.Cause,
            Status = a.Status,
            ProgressPercentage = a.ProgressPercentage,
            Deadline = a.Deadline,
            RealizationMethod = a.RealizationMethod,
            RealizationDate = a.RealizationDate,
            VerificationMethod = a.VerificationMethod,
            VerificationDate = a.VerificationDate,
            Effectiveness = a.Effectiveness,
            EffectivenessComment = a.EffectivenessComment,
            StarRating = a.StarRating,
            CreatedAt = a.CreatedAt,
            ActionPlanId = a.ActionPlanId,
            ResponsibleId = a.ResponsibleId,
            ResponsibleName = a.Responsible?.FullName ?? string.Empty,
            ParentActionId = a.ParentActionId
        };

        private async Task NotifyActionUpdatedAsync(ActionItem action, int actingUserId, List<string> modifications)
        {
            var actor = await _context.Users
                .Where(u => u.Id == actingUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync() ?? "Un utilisateur";

            var plan = await _context.ActionPlans.FirstOrDefaultAsync(p => p.Id == action.ActionPlanId);
            if (plan == null) return;

            var recipientIds = new List<int>();
            if (action.ResponsibleId != actingUserId) recipientIds.Add(action.ResponsibleId);
            if (plan.PilotId != actingUserId) recipientIds.Add(plan.PilotId);

            var details = string.Join(", ", modifications.Distinct());
            foreach (var recipientId in recipientIds.Distinct())
            {
                await _notificationService.CreateInAppAsync(
                    recipientId,
                    "Action modifiee",
                    $"{actor} a modifie l'action \"{action.Theme}\" ({details}).",
                    action.Id);
            }

            if (action.Deadline.Date < DateTime.UtcNow.Date)
            {
                var users = await _context.Users
                    .Where(u => recipientIds.Distinct().Contains(u.Id))
                    .ToListAsync();

                var subject = $"APM — Relance retard: {action.Theme}";
                var bodyBuilder = new StringBuilder();
                bodyBuilder.Append($"<h2>Action en retard</h2>");
                bodyBuilder.Append($"<p>L'action <b>{action.Theme}</b> est en retard depuis le {action.Deadline:dd/MM/yyyy}.</p>");
                bodyBuilder.Append($"<p>Merci de prendre les actions necessaires rapidement.</p>");

                foreach (var user in users)
                {
                    await _emailService.SendEmailAsync(user.Email, user.FullName, subject, bodyBuilder.ToString());
                }
            }
        }
    }
}