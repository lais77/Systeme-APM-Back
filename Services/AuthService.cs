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
            Console.WriteLine($"Email reçu: '{dto.Email}'");
            Console.WriteLine($"Password reçu: '{dto.Password}'");

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            Console.WriteLine($"User trouvé: {(user != null ? "OUI" : "NON")}");

            if (user == null)
                throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

            var bcryptResult = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            Console.WriteLine($"BCrypt result: {bcryptResult}");

            if (!bcryptResult)
                throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

            var (token, expiration) = _jwtHelper.GenerateToken(user);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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