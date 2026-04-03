using APM.API.Data;
using APM.API.DTOs.Admin;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class TeamService
    {
        private readonly AppDbContext _context;

        public TeamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamDto>> GetAllAsync()
        {
            return await _context.Teams
                .Include(t => t.Department)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    DepartmentId = t.DepartmentId,
                    DepartmentName = t.Department != null ? t.Department.Name : null,
                    MemberCount = t.Members.Count
                }).ToListAsync();
        }

        public async Task<TeamDto> CreateAsync(CreateTeamDto dto)
        {
            var team = new Team
            {
                Name = dto.Name,
                DepartmentId = dto.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                DepartmentId = team.DepartmentId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return false;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}