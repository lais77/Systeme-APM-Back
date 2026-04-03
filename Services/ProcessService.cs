using APM.API.Data;
using APM.API.DTOs.Admin;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class ProcessService
    {
        private readonly AppDbContext _context;

        public ProcessService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProcessDto>> GetAllAsync()
        {
            return await _context.Processes
                .Select(p => new ProcessDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    IsActive = p.IsActive
                }).ToListAsync();
        }

        public async Task<ProcessDto> CreateAsync(CreateProcessDto dto)
        {
            var process = new Process
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true
            };
            _context.Processes.Add(process);
            await _context.SaveChangesAsync();

            return new ProcessDto
            {
                Id = process.Id,
                Name = process.Name,
                Description = process.Description,
                IsActive = process.IsActive
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var process = await _context.Processes.FindAsync(id);
            if (process == null) return false;

            _context.Processes.Remove(process);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}