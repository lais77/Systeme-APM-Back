using APM.API.Data;
using APM.API.DTOs.Plans;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class PlanService
    {
        private readonly AppDbContext _context;

        public PlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanDto>> GetAllPlansAsync()
        {
            return await _context.ActionPlans
                .Include(p => p.Pilot)
                .Include(p => p.Process)
                .Include(p => p.Department)
                .Include(p => p.Actions)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<List<PlanDto>> GetMyPlansAsync(int pilotId)
        {
            return await _context.ActionPlans
                .Include(p => p.Pilot)
                .Include(p => p.Process)
                .Include(p => p.Department)
                .Include(p => p.Actions)
                .Where(p => p.PilotId == pilotId)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PlanDto?> GetPlanByIdAsync(int id)
        {
            var plan = await _context.ActionPlans
                .Include(p => p.Pilot)
                .Include(p => p.Process)
                .Include(p => p.Department)
                .Include(p => p.Actions)
                    .ThenInclude(a => a.Responsible)
                .FirstOrDefaultAsync(p => p.Id == id);

            return plan == null ? null : MapToDto(plan);
        }

        public async Task<PlanDto> CreatePlanAsync(CreatePlanDto dto, int pilotId)
        {
            var plan = new ActionPlan
            {
                Title = dto.Title,
                Description = dto.Description,
                Objective = dto.Objective,
                Priority = dto.Priority,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                ProcessId = dto.ProcessId,
                DepartmentId = dto.DepartmentId,
                PilotId = pilotId,
                Status = "InProgress",
                CreatedAt = DateTime.UtcNow
            };

            _context.ActionPlans.Add(plan);
            await _context.SaveChangesAsync();

            return (await GetPlanByIdAsync(plan.Id))!;
        }

        public async Task<PlanDto?> UpdatePlanAsync(int id, UpdatePlanDto dto)
        {
            var plan = await _context.ActionPlans.FindAsync(id);
            if (plan == null) return null;

            if (dto.Title != null) plan.Title = dto.Title;
            if (dto.Description != null) plan.Description = dto.Description;
            if (dto.Objective != null) plan.Objective = dto.Objective;
            if (dto.Priority != null) plan.Priority = dto.Priority;
            if (dto.DueDate.HasValue) plan.DueDate = dto.DueDate.Value;

            await _context.SaveChangesAsync();
            return await GetPlanByIdAsync(id);
        }

        public async Task<bool> ClosePlanAsync(int id)
        {
            var plan = await _context.ActionPlans.FindAsync(id);
            if (plan == null) return false;

            plan.Status = "Closed";
            plan.ClosedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateProgressAsync(int planId)
        {
            var plan = await _context.ActionPlans
                .Include(p => p.Actions)
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null || !plan.Actions.Any()) return;

            plan.ProgressPercentage = plan.Actions.Average(a => a.ProgressPercentage);
            await _context.SaveChangesAsync();
        }

        private static PlanDto MapToDto(ActionPlan p) => new PlanDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Objective = p.Objective,
            Priority = p.Priority,
            Status = p.Status,
            StartDate = p.StartDate,
            DueDate = p.DueDate,
            ProgressPercentage = p.ProgressPercentage,
            CreatedAt = p.CreatedAt,
            ClosedAt = p.ClosedAt,
            PilotId = p.PilotId,
            PilotName = p.Pilot?.FullName ?? string.Empty,
            ProcessId = p.ProcessId,
            ProcessName = p.Process?.Name ?? string.Empty,
            DepartmentId = p.DepartmentId,
            DepartmentName = p.Department?.Name,
            TotalActions = p.Actions?.Count ?? 0
        };
    }
}