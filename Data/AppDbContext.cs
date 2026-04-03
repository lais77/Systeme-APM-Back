using Microsoft.EntityFrameworkCore;

namespace APM.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) 
        { 
        }

        // Les tables seront ajoutées ici dans les prochains sprints
    }
}