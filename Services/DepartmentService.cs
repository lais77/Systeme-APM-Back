using APM.API.Data;
using APM.API.DTOs.Admin;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class DepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentDto
                {
                    id = d.Id,
                    nom = d.Name,
                    description = d.Description,
                    nombreUtilisateurs = d.Users.Count
                }).ToListAsync();
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
        {
            var dept = new Department
            {
                Name = dto.nom,
                Description = dto.description,
                CreatedAt = DateTime.UtcNow
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                id = dept.Id,
                nom = dept.Name,
                description = dept.Description,
                nombreUtilisateurs = 0
            };
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return null;

            dept.Name = dto.nom;
            dept.Description = dto.description;
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                id = dept.Id,
                nom = dept.Name,
                description = dept.Description,
                nombreUtilisateurs = dept.Users.Count
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}