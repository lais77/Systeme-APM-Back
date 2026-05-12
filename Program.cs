using APM.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NSwag.AspNetCore;
using APM.API.Services;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "APM API — TIS Circuits";
    config.Version = "v1";
    config.Description = "Système de gestion des plans d'action PDCA";
    config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Entrez : Bearer {votre token}"
    });
    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.OperationSecurityScopeProcessor("JWT"));
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EscaladeService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddControllers();
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<AttachmentService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowed != null && allowed.Length > 0)
        {
            policy.WithOrigins(allowed)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost",
                "http://localhost:80",
                "http://frontend"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Auto-migration au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await APM.API.Data.DbSeeder.SeedAsync(db);
}


// CORS en premier !
app.UseCors("AllowAngular");

app.UseHangfireDashboard("/hangfire");
RecurringJob.AddOrUpdate<EscaladeService>(
    "escalade-quotidienne",
    service => service.RunDailyCheckAsync(),
    Cron.Daily);

app.UseOpenApi();
app.UseSwaggerUi(c =>
{
    c.DocumentTitle = "APM API — TIS Circuits";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health Check endpoint — Monitoring APM
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var dbOk = await db.Database.CanConnectAsync();

        return Results.Ok(new
        {
            status = dbOk ? "Healthy" : "Degraded",
            database = dbOk ? "Connected" : "Unreachable",
            hangfire = "Running",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            timestamp = DateTime.UtcNow,
            version = "2.1.0",
            application = "APM — TIS Circuits"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "Unhealthy",
            database = "Error",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        }, statusCode: 503);
    }
}).AllowAnonymous();

app.Run();