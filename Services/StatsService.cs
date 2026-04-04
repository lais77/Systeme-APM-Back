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

        public async Task<GlobalStatsDto> GetGlobalStatsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var totalPlans = await _context.ActionPlans.CountAsync();
            var totalActions = await _context.ActionItems.CountAsync();
            var enCours = await _context.ActionItems.CountAsync(a => a.Status == "InProgress");
            var cloturees = await _context.ActionItems.CountAsync(a => a.Status == "Clôturé");
            var enRetard = await _context.ActionItems.CountAsync(a =>
                a.Status != "Clôturé" && a.Status != "Annulé" && a.Deadline.Date < today);
            var efficaces = await _context.ActionItems.CountAsync(a => a.Effectiveness == "Efficace");

            return new GlobalStatsDto
            {
                totalPlans = totalPlans,
                totalActions = totalActions,
                actionsEnCours = enCours,
                actionsEnRetard = enRetard,
                actionsCloturees = cloturees,
                tauxRealisation = totalActions > 0 ? Math.Round((double)enCours / totalActions * 100, 1) : 0,
                tauxCloture = totalActions > 0 ? Math.Round((double)cloturees / totalActions * 100, 1) : 0,
                tauxEfficacite = cloturees > 0 ? Math.Round((double)efficaces / cloturees * 100, 1) : 0
            };
        }

        public async Task<List<StatsByDepartmentDto>> GetStatsByDepartmentAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            var result = new List<StatsByDepartmentDto>();

            foreach (var d in departments)
            {
                var plans = await _context.ActionPlans
                    .Where(p => p.DepartmentId == d.Id)
                    .ToListAsync();

                var planIds = plans.Select(p => p.Id).ToList();

                var actions = await _context.ActionItems
                    .Where(a => planIds.Contains(a.ActionPlanId))
                    .ToListAsync();

                var cloturees = actions.Count(a => a.Status == "Clôturé");

                result.Add(new StatsByDepartmentDto
                {
                    departmentName = d.Name,
                    totalPlans = plans.Count,
                    totalActions = actions.Count,
                    actionsCloturees = cloturees,
                    tauxCloture = actions.Count > 0
                        ? Math.Round((double)cloturees / actions.Count * 100, 1)
                        : 0
                });
            }

            return result;
        }

        public async Task<List<StatsByPilotDto>> GetStatsByPilotAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "MANAGER")
                .Select(u => new StatsByPilotDto
                {
                    pilotName = u.FullName,
                    totalPlans = u.ManagedPlans.Count(),
                    totalActions = u.ManagedPlans.SelectMany(p => p.Actions).Count(),
                    tauxCloture = u.ManagedPlans.SelectMany(p => p.Actions).Count() > 0
                        ? Math.Round((double)u.ManagedPlans.SelectMany(p => p.Actions)
                            .Count(a => a.Status == "Clôturé") /
                            u.ManagedPlans.SelectMany(p => p.Actions).Count() * 100, 1)
                        : 0
                }).ToListAsync();
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

            var enCours = actions.Count(a => a.Status == "InProgress");
            var cloturees = actions.Count(a => a.Status == "Clôturé");
            var enRetard = actions.Count(a => a.Status != "Clôturé" && a.Status != "Annulé" && a.Deadline < end);
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
            var cloturees = await _context.ActionItems.CountAsync(a => a.Status == "Clôturé");
            var enCours = await _context.ActionItems.CountAsync(a => a.Status == "InProgress");
            var efficaces = await _context.ActionItems.CountAsync(a => a.Effectiveness == "Efficace");
            var enRetard = await _context.ActionItems.CountAsync(a =>
                a.Status != "Clôturé" && a.Status != "Annulé" && a.Deadline.Date < today);

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
                    a.Status == "Clôturé" &&
                    a.RealizationDate.HasValue &&
                    a.RealizationDate.Value.Month == month &&
                    a.RealizationDate.Value.Year == year),
                actionsEnRetard = actions.Count(a =>
                    a.Status != "Clôturé" &&
                    a.Deadline.Month == month &&
                    a.Deadline.Year == year &&
                    a.Deadline.Date < today)
            }).ToList();
        }
    }
}