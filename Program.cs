using APM.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Connexion à la base de données
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "APM API — TIS Circuits",
        Version = "v1",
        Description = "Système de gestion des plans d'action PDCA"
    });
});

// 3. Contrôleurs
builder.Services.AddControllers();

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger toujours actif (pas seulement en développement)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "APM API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();