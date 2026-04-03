using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Si la table Users a déjà des données, on ne fait rien
            if (await context.Users.AnyAsync())
                return;

            // ===== CRÉER LES DÉPARTEMENTS =====
            var deptInfo = new Department
            {
                Name = "Service Système d'Information",
                Description = "IT et Développement"
            };
            var deptQuality = new Department
            {
                Name = "DQSSE",
                Description = "Qualité, Sécurité, Environnement"
            };
            var deptProd = new Department
            {
                Name = "Production",
                Description = "Fabrication"
            };

            context.Departments.AddRange(deptInfo, deptQuality, deptProd);
            await context.SaveChangesAsync();

            // ===== CRÉER LES PROCESSUS =====
            context.Processes.AddRange(
                new Process { Name = "Informatique" },
                new Process { Name = "Qualité" },
                new Process { Name = "Production" },
                new Process { Name = "Maintenance" }
            );
            await context.SaveChangesAsync();

            // ===== CRÉER LES UTILISATEURS =====
            // Le mot de passe est hashé avec BCrypt
            var admin = new User
            {
                FullName = "Admin APM",
                Email = "admin@tiscircuits.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                DepartmentId = deptInfo.Id
            };

            var manager = new User
            {
                FullName = "Chadli BEDDEY",
                Email = "chadli@tiscircuits.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                Role = "Manager",
                DepartmentId = deptInfo.Id
            };

            var responsable = new User
            {
                FullName = "Ahmed BEN ALI",
                Email = "ahmed@tiscircuits.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Resp@123"),
                Role = "Responsable",
                DepartmentId = deptInfo.Id
            };

            var auditeur = new User
            {
                FullName = "Directeur USINE",
                Email = "direction@tiscircuits.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Audit@123"),
                Role = "Auditeur",
                DepartmentId = deptProd.Id
            };

            context.Users.AddRange(admin, manager, responsable, auditeur);
            await context.SaveChangesAsync();

            // Lier le responsable à son chef
            responsable.ManagerId = manager.Id;
            await context.SaveChangesAsync();
        }
    }
}