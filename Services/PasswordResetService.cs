using APM.API.Data;
using APM.API.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class PasswordResetService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        // Stockage temporaire des tokens (en production → BDD)
        private static readonly Dictionary<string, (string email, DateTime expiry)> _tokens = new();

        public PasswordResetService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> SendResetLinkAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user == null) return false;

            // Générer token unique
            var token = Guid.NewGuid().ToString("N");
            _tokens[token] = (dto.Email, DateTime.UtcNow.AddHours(1));

            var resetLink = $"http://localhost:4200/reset-password?token={token}&email={dto.Email}";

            var subject = "APM — Réinitialisation de votre mot de passe";
            var body = $@"
                <h2>Réinitialisation du mot de passe</h2>
                <p>Bonjour {user.FullName},</p>
                <p>Cliquez sur le lien ci-dessous pour réinitialiser votre mot de passe :</p>
                <a href='{resetLink}' style='background:#2B5FA3;color:white;padding:10px 20px;border-radius:5px;text-decoration:none'>
                    Réinitialiser mon mot de passe
                </a>
                <p>Ce lien expire dans 1 heure.</p>
                <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>";

            await _emailService.SendEmailAsync(user.Email, user.FullName, subject, body);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            // Vérifier le token
            if (!_tokens.TryGetValue(dto.Token, out var data))
                return false;

            if (data.email != dto.Email || data.expiry < DateTime.UtcNow)
            {
                _tokens.Remove(dto.Token);
                return false;
            }

            // Mettre à jour le mot de passe
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            _tokens.Remove(dto.Token);
            return true;
        }
    }
}