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
                    id = p.Id,
                    nom = p.Name,
                    description = p.Description,
                    actif = p.IsActive
                }).ToListAsync();
        }

        public async Task<ProcessDto> CreateAsync(CreateProcessDto dto)
        {
            var process = new Process
            {
                Name = dto.nom,
                Description = dto.description,
                IsActive = true
            };
            _context.Processes.Add(process);
            await _context.SaveChangesAsync();

            return new ProcessDto
            {
                id = process.Id,
                nom = process.Name,
                description = process.Description,
                actif = process.IsActive
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