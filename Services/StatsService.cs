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
                TotalPlans = totalPlans,
                TotalActions = totalActions,
                ActionsEnCours = enCours,
                ActionsEnRetard = enRetard,
                ActionsCloturees = cloturees,
                TauxRealisation = totalActions > 0 ? Math.Round((double)enCours / totalActions * 100, 1) : 0,
                TauxCloture = totalActions > 0 ? Math.Round((double)cloturees / totalActions * 100, 1) : 0,
                TauxEfficacite = cloturees > 0 ? Math.Round((double)efficaces / cloturees * 100, 1) : 0
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
                    DepartmentName = d.Name,
                    TotalPlans = plans.Count,
                    TotalActions = actions.Count,
                    ActionsCloturees = cloturees,
                    TauxCloture = actions.Count > 0
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
                    PilotName = u.FullName,
                    TotalPlans = u.ManagedPlans.Count(),
                    TotalActions = u.ManagedPlans.SelectMany(p => p.Actions).Count(),
                    TauxCloture = u.ManagedPlans.SelectMany(p => p.Actions).Count() > 0
                        ? Math.Round((double)u.ManagedPlans.SelectMany(p => p.Actions)
                            .Count(a => a.Status == "Clôturé") /
                            u.ManagedPlans.SelectMany(p => p.Actions).Count() * 100, 1)
                        : 0
                }).ToListAsync();
        }

        public async Task<List<MonthlyStatsDto>> GetMonthlyStatsAsync(int year)
        {
            var actions = await _context.ActionItems.ToListAsync();
            var today = DateTime.UtcNow.Date;

            return Enumerable.Range(1, 12).Select(month => new MonthlyStatsDto
            {
                Month = month,
                Year = year,
                ActionsCloturees = actions.Count(a =>
                    a.Status == "Clôturé" &&
                    a.RealizationDate.HasValue &&
                    a.RealizationDate.Value.Month == month &&
                    a.RealizationDate.Value.Year == year),
                ActionsEnRetard = actions.Count(a =>
                    a.Status != "Clôturé" &&
                    a.Deadline.Month == month &&
                    a.Deadline.Year == year &&
                    a.Deadline.Date < today)
            }).ToList();
        }
    }
}