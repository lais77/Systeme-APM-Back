using APM.API.Data;
using APM.API.DTOs.Stats;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class StatsService
    {
        private readonly AppDbContext _context;

        public StatsService(AppDbContext context)
        {
            _context = context;
        }

        // Helpers centralisés — toute la logique statuts ici
        private static readonly string[] ClosedStatuses = { "Clôturé", "Closed", "C" };
        private static readonly string[] ActiveStatuses = { "InProgress", "D", "UnderReview", "Validated", "P", "Created", "Assigned" };
        private static readonly string[] CancelledStatuses = { "Annulé", "Cancelled" };

        private bool IsClosedStatus(string status) => ClosedStatuses.Contains(status);
        private bool IsActiveStatus(string status) => ActiveStatuses.Contains(status);
        private bool IsCancelledStatus(string status) => CancelledStatuses.Contains(status);

        public async Task<GlobalStatsDto> GetGlobalStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var totalActions = await _context.ActionItems.CountAsync();

            var cloturees = await _context.ActionItems
                .CountAsync(a => ClosedStatuses.Contains(a.Status));

            var enRetard = await _context.ActionItems
                .CountAsync(a => 
                    !ClosedStatuses.Contains(a.Status) && 
                    !CancelledStatuses.Contains(a.Status) &&
                    a.Deadline.Date < today);

            var enCours = await _context.ActionItems
                .CountAsync(a => ActiveStatuses.Contains(a.Status));

            var efficaces = await _context.ActionItems
                .CountAsync(a => a.Effectiveness == "Efficace");

            return new GlobalStatsDto
            {
                totalPlans = await _context.ActionPlans.CountAsync(),
                totalActions = totalActions,
                actionsEnCours = enCours,
                actionsCloturees = cloturees,
                actionsEnRetard = enRetard,
                tauxRealisation = totalActions > 0 ? Math.Round((double)enCours / totalActions * 100, 1) : 0,
                tauxCloture = totalActions > 0 ? Math.Round((double)cloturees / totalActions * 100, 1) : 0,
                tauxEfficacite = cloturees > 0 ? Math.Round((double)efficaces / cloturees * 100, 1) : 0
            };
        }

        public async Task<List<StatsByDeptDto>> GetStatsByDepartmentAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.ActionPlans
                .Include(p => p.Department)
                .Include(p => p.Actions)
                .GroupBy(p => new { 
                    DeptId = p.DepartmentId ?? 0, 
                    DeptName = p.Department != null ? p.Department.Name : "Sans département" 
                })
                .Select(g => new StatsByDeptDto
                {
                    departmentId = g.Key.DeptId,
                    departmentName = g.Key.DeptName,
                    totalPlans = g.Count(),
                    totalActions = g.Sum(p => p.Actions.Count),
                    actionsEnRetard = g.Sum(p => p.Actions.Count(a =>
                        !ClosedStatuses.Contains(a.Status) &&
                        a.Deadline.Date < today)),
                    actionsCloturees = g.Sum(p => p.Actions.Count(a =>
                        ClosedStatuses.Contains(a.Status)))
                })
                .ToListAsync();
        }

        public async Task<List<StatsByPilotDto>> GetStatsByPilotAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.ActionPlans
                .Include(p => p.Pilot)
                .Include(p => p.Actions)
                .GroupBy(p => new { 
                    PilotId = p.PilotId, 
                    PilotName = p.Pilot.FullName 
                })
                .Select(g => new StatsByPilotDto
                {
                    pilotId = g.Key.PilotId,
                    pilotName = g.Key.PilotName,
                    totalPlans = g.Count(),
                    actionsEnRetard = g.Sum(p => p.Actions.Count(a =>
                        !ClosedStatuses.Contains(a.Status) &&
                        a.Deadline.Date < today)),
                    actionsCloturees = g.Sum(p => p.Actions.Count(a =>
                        ClosedStatuses.Contains(a.Status)))
                })
                .ToListAsync();
        }

        public async Task<GlobalStatsDto> GetStatsByPeriodAsync(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                startDate = new DateTime(today.Year, today.Month, 1);
                endDate = today;
            }

            var start = startDate.Value;
            var end = endDate.Value;
            var plans = await _context.ActionPlans
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            var planIds = plans.Select(p => p.Id).ToList();

            var actions = await _context.ActionItems
                .Where(a => planIds.Contains(a.ActionPlanId))
                .ToListAsync();

            var enCours = actions.Count(a => ActiveStatuses.Contains(a.Status));
            var cloturees = actions.Count(a => ClosedStatuses.Contains(a.Status));
            var enRetard = actions.Count(a => !ClosedStatuses.Contains(a.Status) && !CancelledStatuses.Contains(a.Status) && a.Deadline < end);
            var efficaces = actions.Count(a => a.Effectiveness == "Efficace");

            return new GlobalStatsDto
            {
                totalPlans = plans.Count,
                totalActions = actions.Count,
                actionsEnCours = enCours,
                actionsEnRetard = enRetard,
                actionsCloturees = cloturees,
                tauxRealisation = actions.Count > 0 ? Math.Round((double)enCours / actions.Count * 100, 1) : 0,
                tauxCloture = actions.Count > 0 ? Math.Round((double)cloturees / actions.Count * 100, 1) : 0,
                tauxEfficacite = cloturees > 0 ? Math.Round((double)efficaces / cloturees * 100, 1) : 0
            };
        }

        public async Task<Dictionary<string, double>> GetPerformanceStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var totalActions = await _context.ActionItems.CountAsync();
            var cloturees = await _context.ActionItems.CountAsync(a => ClosedStatuses.Contains(a.Status));
            var enCours = await _context.ActionItems.CountAsync(a => ActiveStatuses.Contains(a.Status));
            var efficaces = await _context.ActionItems.CountAsync(a => a.Effectiveness == "Efficace");
            var enRetard = await _context.ActionItems.CountAsync(a =>
                !ClosedStatuses.Contains(a.Status) && !CancelledStatuses.Contains(a.Status) && a.Deadline.Date < today);

            var totalActionsAvecDeadline = await _context.ActionItems.CountAsync(a => a.Deadline != null);
            var aTemps = totalActionsAvecDeadline > 0
                ? await _context.ActionItems.CountAsync(a => a.RealizationDate != null && a.RealizationDate <= a.Deadline)
                : 0;

            return new Dictionary<string, double>
            {
                ["tauxRealisation"] = totalActions > 0 ? Math.Round((double)cloturees / totalActions * 100, 1) : 0,
                ["tauxEfficacite"] = cloturees > 0 ? Math.Round((double)efficaces / cloturees * 100, 1) : 0,
                ["tauxRetard"] = totalActions > 0 ? Math.Round((double)enRetard / totalActions * 100, 1) : 0,
                ["tauxATemps"] = totalActionsAvecDeadline > 0 ? Math.Round((double)aTemps / totalActionsAvecDeadline * 100, 1) : 0,
                ["actionEnCours"] = enCours
            };
        }

        public async Task<List<MonthlyStatsDto>> GetMonthlyStatsAsync(int year)
        {
            var actions = await _context.ActionItems.ToListAsync();
            var today = DateTime.UtcNow.Date;

            return Enumerable.Range(1, 12).Select(month => new MonthlyStatsDto
            {
                month = month,
                year = year,
                actionsCloturees = actions.Count(a =>
                    ClosedStatuses.Contains(a.Status) &&
                    a.RealizationDate.HasValue &&
                    a.RealizationDate.Value.Month == month &&
                    a.RealizationDate.Value.Year == year),
                actionsEnRetard = actions.Count(a =>
                    !ClosedStatuses.Contains(a.Status) &&
                    !CancelledStatuses.Contains(a.Status) &&
                    a.Deadline.Month == month &&
                    a.Deadline.Year == year &&
                    a.Deadline.Date < today)
            }).ToList();
        }

        public async Task<List<PlanCritiqueDto>> GetPlansCritiquesAsync()
        {
            var today = DateTime.UtcNow.Date;

            var plans = await _context.ActionPlans
                .Include(p => p.Actions)
                .Include(p => p.Pilot)
                .Include(p => p.Department)
                .Where(p => !ClosedStatuses.Contains(p.Status) && !CancelledStatuses.Contains(p.Status))
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Status,
                    p.Priority,
                    p.DueDate,
                    PilotName = p.Pilot.FullName,
                    DepartmentName = p.Department != null ? p.Department.Name : "—",
                    ActionsEnRetard = p.Actions.Count(a =>
                        !ClosedStatuses.Contains(a.Status) &&
                        !CancelledStatuses.Contains(a.Status) &&
                        a.Deadline.Date < today),
                    TotalActions = p.Actions.Count
                })
                .Where(p => p.ActionsEnRetard > 0 || p.DueDate.Date <= today.AddDays(3))
                .OrderByDescending(p => p.ActionsEnRetard)
                .Take(5)
                .ToListAsync();

            return plans.Select(p => new PlanCritiqueDto
            {
                id = p.Id,
                title = p.Title,
                status = p.Status,
                priority = p.Priority,
                pilotName = p.PilotName,
                departmentName = p.DepartmentName,
                actionsEnRetard = p.ActionsEnRetard,
                totalActions = p.TotalActions,
                dateEcheance = p.DueDate
            }).ToList();
        }

        public async Task<List<object>> GetActiviteRecenteAsync()
        {
            return await _context.ActivityLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new {
                    id = a.Id,
                    action = a.Action,
                    entityType = a.EntityType,
                    userName = a.User.FullName,
                    createdAt = a.CreatedAt
                })
                .Cast<object>()
                .ToListAsync();
        }
    }
}