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
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    UserCount = d.Users.Count
                }).ToListAsync();
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
        {
            var dept = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name,
                Description = dept.Description,
                UserCount = 0
            };
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return null;

            dept.Name = dto.Name;
            dept.Description = dto.Description;
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name,
                Description = dept.Description
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