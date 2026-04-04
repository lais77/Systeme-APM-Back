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
                    id = u.Id,
                    nom = u.FullName,
                    prenom = "",
                    email = u.Email,
                    role = u.Role,
                    actif = u.IsActive,
                    departement = u.Department != null ? u.Department.Name : null,
                    equipe = u.Team != null ? u.Team.Name : null,
                    chef = u.Manager != null ? u.Manager.FullName : null,
                    dateCreation = u.CreatedAt,
                    dernierLogin = u.LastLoginAt
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
                id = u.Id,
                nom = u.FullName,
                prenom = "",
                email = u.Email,
                role = u.Role,
                actif = u.IsActive,
                departement = u.Department?.Name,
                equipe = u.Team?.Name,
                chef = u.Manager?.FullName,
                dateCreation = u.CreatedAt,
                dernierLogin = u.LastLoginAt
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.email))
                throw new InvalidOperationException("Email déjà utilisé.");

            var user = new User
            {
                FullName = dto.nom,
                Email = dto.email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
                Role = dto.role,
                DepartmentId = dto.departementId,
                TeamId = dto.equipeId,
                ManagerId = dto.chefId,
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

            user.FullName = dto.nom;
            user.Role = dto.role;
            user.DepartmentId = dto.departementId;
            user.TeamId = dto.equipeId;
            user.ManagerId = dto.chefId;
            user.IsActive = dto.actif;

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