using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using APM.API.Data;
using APM.API.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IHttpClientFactory httpClientFactory,
            AppDbContext db,
            IConfiguration configuration,
            ILogger<ChatController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Le message est requis." });

            var apiKey = _configuration["Claude:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return BadRequest(new { message = "Assistant IA non configuré (Claude:ApiKey manquante)." });

            var model = _configuration["Claude:Model"] ?? "claude-sonnet-4-20250514";
            var maxTokens = int.TryParse(_configuration["Claude:MaxTokens"], out var mt) ? mt : 1024;

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var context = await BuildContextAsync(userId, userRole);

            var systemPrompt = $"""
                Tu es l'assistant IA du système APM (Action Plan Management) de TIS Circuits.
                Tu aides les utilisateurs à comprendre et gérer leurs plans d'action PDCA.
                Réponds toujours en français, de façon concise et professionnelle.

                DONNÉES ACTUELLES DE L'UTILISATEUR (rôle : {userRole}) :
                {context}

                Règles :
                - Réponds uniquement à partir des données fournies ci-dessus
                - Si une information n'est pas dans les données, dis-le clairement
                - Ne génère jamais de fausses données
                """;

            var payload = new ClaudeApiRequestDto
            {
                Model = model,
                MaxTokens = maxTokens,
                System = systemPrompt,
                Messages =
                [
                    new ClaudeUserMessageDto { Role = "user", Content = request.Message.Trim() }
                ]
            };

            var client = _httpClientFactory.CreateClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            httpRequest.Headers.TryAddWithoutValidation("x-api-key", apiKey);
            httpRequest.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Content = JsonContent.Create(payload);

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec d'appel HTTP vers l'API Claude.");
                return StatusCode(502, new { message = "Impossible de joindre le service Claude." });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Claude API {Status}: {Body}", response.StatusCode, errBody);
                return StatusCode((int)response.StatusCode, new { message = "Erreur API Claude.", detail = errBody });
            }

            var result = await response.Content.ReadFromJsonAsync<ClaudeMessageResponseDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var text = result?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;
            if (string.IsNullOrEmpty(text))
                return StatusCode(502, new { message = "Réponse Claude vide ou inattendue." });

            return Ok(new { reply = text });
        }

        private async Task<string> BuildContextAsync(int userId, string role)
        {
            var today = DateTime.UtcNow.Date;
            var roleUpper = role.ToUpperInvariant();

            if (roleUpper == "RESPONSABLE")
            {
                var actions = await _db.ActionItems
                    .AsNoTracking()
                    .Where(a => a.ResponsibleId == userId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Theme,
                        a.ActionDescription,
                        a.Status,
                        a.Deadline,
                        EnRetard = a.Status != "Clôturé" && a.Status != "Annulé" && a.Deadline.Date < today
                    })
                    .ToListAsync();

                return $"Actions assignées (responsable) : {JsonSerializer.Serialize(actions)}";
            }

            if (roleUpper == "MANAGER")
            {
                var plans = await _db.ActionPlans
                    .AsNoTracking()
                    .Where(p => p.PilotId == userId)
                    .Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.Status,
                        TotalActions = p.Actions.Count,
                        ActionsEnRetard = p.Actions.Count(a =>
                            a.Status != "Clôturé" && a.Status != "Annulé" && a.Deadline.Date < today)
                    })
                    .ToListAsync();

                return $"Plans pilotés : {JsonSerializer.Serialize(plans)}";
            }

            if (roleUpper == "ADMIN" || roleUpper == "AUDITEUR")
            {
                var summary = new
                {
                    totalPlans = await _db.ActionPlans.CountAsync(),
                    totalActions = await _db.ActionItems.CountAsync(),
                    actionsEnCours = await _db.ActionItems.CountAsync(a => a.Status == "InProgress"),
                    actionsCloturees = await _db.ActionItems.CountAsync(a => a.Status == "Clôturé"),
                    actionsEnRetard = await _db.ActionItems.CountAsync(a =>
                        a.Status != "Clôturé" && a.Status != "Annulé" && a.Deadline.Date < today)
                };

                return $"Vue synthétique (tous les plans / actions) : {JsonSerializer.Serialize(summary)}";
            }

            return JsonSerializer.Serialize(new { message = "Rôle non reconnu pour le contexte métier.", role });
        }
    }
}
