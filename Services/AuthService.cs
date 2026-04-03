using APM.API.Data;
using APM.API.DTOs.Auth;
using APM.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _jwtHelper = new JwtHelper(config);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. Chercher l'utilisateur par email
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            // 2. Si pas trouvé → erreur
            if (user == null)
                throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

            // 3. Vérifier le mot de passe
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

            // 4. Générer le token JWT
            var (token, expiration) = _jwtHelper.GenerateToken(user);

            // 5. Mettre à jour la date du dernier login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 6. Retourner le résultat
            return new TokenResponseDto
            {
                Token = token,
                Expiration = expiration,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    DepartmentName = user.Department?.Name
                }
            };
        }
    }
}