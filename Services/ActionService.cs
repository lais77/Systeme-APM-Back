using APM.API.Data;
using APM.API.DTOs.Actions;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class ActionService
    {
        private readonly AppDbContext _context;
        private readonly PlanService _planService;

        public ActionService(AppDbContext context, PlanService planService)
        {
            _context = context;
            _planService = planService;
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

        public async Task<ActionDto> CreateActionAsync(int planId, CreateActionDto dto)
        {
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
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            _context.ActionItems.Add(action);
            await _context.SaveChangesAsync();
            await _planService.UpdateProgressAsync(planId);

            return (await GetActionByIdAsync(action.Id))!;
        }

        public async Task<ActionDto?> UpdateActionAsync(int id, UpdateActionDto dto)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null) return null;

            if (dto.Theme != null) action.Theme = dto.Theme;
            if (dto.ActionDescription != null) action.ActionDescription = dto.ActionDescription;
            if (dto.Type != null) action.Type = dto.Type;
            if (dto.Criticity != null) action.Criticity = dto.Criticity;
            if (dto.Cause != null) action.Cause = dto.Cause;
            if (dto.Deadline.HasValue) action.Deadline = dto.Deadline.Value;
            if (dto.ResponsibleId.HasValue) action.ResponsibleId = dto.ResponsibleId.Value;
            if (dto.ProgressPercentage.HasValue) action.ProgressPercentage = dto.ProgressPercentage.Value;

            action.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _planService.UpdateProgressAsync(action.ActionPlanId);

            return await GetActionByIdAsync(id);
        }

        public async Task<bool> DeleteActionAsync(int id)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null) return false;

            var planId = action.ActionPlanId;
            _context.ActionItems.Remove(action);
            await _context.SaveChangesAsync();
            await _planService.UpdateProgressAsync(planId);
            return true;
        }

        public async Task<ActionDto?> DemarrerActionAsync(int id)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || action.Status != "Created") return null;

            action.Status = "InProgress";
            action.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetActionByIdAsync(id);
        }

        public async Task<ActionDto?> SubmitActionAsync(int id, SubmitActionDto dto)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || action.Status != "InProgress") return null;

            action.Status = "UnderReview";
            action.RealizationMethod = dto.RealizationMethod;
            action.RealizationDate = dto.RealizationDate;
            action.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetActionByIdAsync(id);
        }

        public async Task<ActionDto?> ValidateActionAsync(int id, ValidateActionDto dto)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || action.Status != "UnderReview") return null;

            action.Status = dto.IsApproved ? "Validated" : "Rejected";
            if (!dto.IsApproved && dto.Comment != null)
                action.EffectivenessComment = dto.Comment;

            action.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetActionByIdAsync(id);
        }

        public async Task<ActionDto?> EvaluateActionAsync(int id, EvaluateActionDto dto)
        {
            var action = await _context.ActionItems.FindAsync(id);
            if (action == null || action.Status != "Validated") return null;

            action.Effectiveness = dto.Effectiveness;
            action.EffectivenessComment = dto.Comment;
            action.StarRating = dto.StarRating;
            action.ModifiedAt = DateTime.UtcNow;

            if (dto.Effectiveness == "Ineffective" && dto.ReplacementAction != null)
            {
                action.Status = "Closed";
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
                    Status = "Created",
                    CreatedAt = DateTime.UtcNow
                };
                _context.ActionItems.Add(replacement);
            }
            else
            {
                action.Status = "Closed";
                action.ProgressPercentage = 100;
            }

            await _context.SaveChangesAsync();
            await _planService.UpdateProgressAsync(action.ActionPlanId);
            return await GetActionByIdAsync(id);
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
    }
}