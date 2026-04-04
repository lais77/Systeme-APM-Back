using APM.API.Data;
using APM.API.DTOs.Admin;
using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .Include(u => u.Manager)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Nom = u.FullName,
                    Prenom = "",
                    Email = u.Email,
                    Role = u.Role,
                    Actif = u.IsActive,
                    Department = u.Department != null ? u.Department.Name : null,
                    TeamName = u.Team != null ? u.Team.Name : null,
                    ManagerName = u.Manager != null ? u.Manager.FullName : null,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                }).ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var u = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Nom = u.FullName,
                Prenom = "",
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                Actif = u.IsActive,
                Department = u.Department?.Name,
                TeamName = u.Team?.Name,
                ManagerName = u.Manager?.FullName,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email déjà utilisé.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                TeamId = dto.TeamId,
                ManagerId = dto.ManagerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(user.Id) ?? throw new Exception("Erreur création.");
        }

        public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FullName = dto.FullName;
            user.Role = dto.Role;
            user.DepartmentId = dto.DepartmentId;
            user.TeamId = dto.TeamId;
            user.ManagerId = dto.ManagerId;
            user.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}