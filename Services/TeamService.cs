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
                    id = t.Id,
                    nom = t.Name,
                    departementId = t.DepartmentId,
                    departement = t.Department != null ? t.Department.Name : null,
                    nombreMembres = t.Members.Count
                }).ToListAsync();
        }

        public async Task<TeamDto> CreateAsync(CreateTeamDto dto)
        {
            var team = new Team
            {
                Name = dto.nom,
                DepartmentId = dto.departementId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return new TeamDto
            {
                id = team.Id,
                nom = team.Name,
                departementId = team.DepartmentId,
                departement = team.Department != null ? team.Department.Name : null,
                nombreMembres = team.Members.Count
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