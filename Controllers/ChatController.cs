// Controllers/ChatController.cs
using System.Security.Claims;
using System.Text.Json;
using APM.API.Data;
using APM.API.DTOs.Chat;
using APM.API.Services;
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
        private readonly IAIService _ai;
        private readonly AppDbContext _db;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IAIService ai, AppDbContext db, ILogger<ChatController> logger)
        {
            _ai = ai;
            _db = db;
            _logger = logger;
        }

        // Endpoint existant : chatbot conversationnel
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Le message est requis." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var context = await BuildContextAsync(userId, userRole);

            var systemPrompt = $"""
                Tu es Alex, l'assistant IA intelligent et sympathique du système APM 
                de TIS Circuits. Tu es professionnel mais chaleureux, et tu t'exprimes 
                toujours en français.

                PROFIL DE L'UTILISATEUR CONNECTÉ :
                - Rôle : {userRole}

                DONNÉES EN TEMPS RÉEL :
                {context}

                CE QUE TU PEUX FAIRE :
                ✅ Répondre aux salutations et faire la conversation
                ✅ Expliquer la méthode PDCA et les bonnes pratiques qualité
                ✅ Analyser les plans et actions de l'utilisateur
                ✅ Identifier les retards et alerter sur les urgences
                ✅ Donner des conseils pour améliorer la gestion des plans
                ✅ Répondre aux questions générales sur l'APM
                ✅ Suggérer des actions correctives

                RÈGLES :
                - Pour les données précises (nombres, noms) : utilise uniquement les données fournies
                - Pour les conseils, explications et conversation : réponds librement
                - Sois concis — maximum 4-5 lignes par réponse
                - Si tu ne sais pas quelque chose, dis-le honnêtement
                - Ne génère jamais de fausses données chiffrées
                """;

            try
            {
                var reply = await _ai.AskAsync(request.Message, systemPrompt);
                return Ok(new { reply });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur appel IA.");
                return StatusCode(502, new { message = ex.Message });
            }
        }

        // Nouvel endpoint : suggestions IA pour un plan
        [HttpGet("plans/{planId:int}/suggestions")]
        public async Task<IActionResult> GetSuggestions(int planId)
        {
            try
            {
                var suggestions = await _ai.SuggestActionsAsync(planId);
                return Ok(new { suggestions });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur suggestions IA.");
                return StatusCode(502, new { message = ex.Message });
            }
        }

        // Nouvel endpoint : résumé de clôture IA
        [HttpGet("plans/{planId:int}/resume")]
        public async Task<IActionResult> GetResume(int planId)
        {
            try
            {
                var resume = await _ai.SummarizePlanAsync(planId);
                return Ok(new { resume });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur résumé IA.");
                return StatusCode(502, new { message = ex.Message });
            }
        }

        // BuildContextAsync — identique à l'original, inchangé
        private async Task<string> BuildContextAsync(int userId, string role)
        {
            var today = DateTime.UtcNow.Date;
            var roleUpper = role.ToUpperInvariant();

            if (roleUpper == "RESPONSABLE")
            {
                var actions = await _db.ActionItems
                    .AsNoTracking()
                    .Where(a => a.ResponsibleId == userId)
                    .Select(a => new {
                        a.Id, a.Theme, a.ActionDescription, a.Status, a.Deadline,
                        EnRetard = a.Status != "Clôturé" && a.Status != "Annulé"
                                   && a.Deadline.Date < today
                    })
                    .ToListAsync();
                return $"Actions assignées : {JsonSerializer.Serialize(actions)}";
            }

            if (roleUpper == "MANAGER")
            {
                var plans = await _db.ActionPlans
                    .AsNoTracking()
                    .Where(p => p.PilotId == userId)
                    .Select(p => new {
                        p.Id, p.Title, p.Status,
                        TotalActions = p.Actions.Count,
                        ActionsEnRetard = p.Actions.Count(a =>
                            a.Status != "Clôturé" && a.Status != "Annulé"
                            && a.Deadline.Date < today)
                    })
                    .ToListAsync();
                return $"Plans pilotés : {JsonSerializer.Serialize(plans)}";
            }

            if (roleUpper is "ADMIN" or "AUDITEUR")
            {
                var summary = new {
                    totalPlans    = await _db.ActionPlans.CountAsync(),
                    totalActions  = await _db.ActionItems.CountAsync(),
                    actionsEnCours = await _db.ActionItems
                        .CountAsync(a => a.Status == "InProgress"),
                    actionsCloturees = await _db.ActionItems
                        .CountAsync(a => a.Status == "Clôturé"),
                    actionsEnRetard = await _db.ActionItems
                        .CountAsync(a => a.Status != "Clôturé"
                                      && a.Status != "Annulé"
                                      && a.Deadline.Date < today)
                };
                return $"Vue synthétique : {JsonSerializer.Serialize(summary)}";
            }

            return JsonSerializer.Serialize(new { message = "Rôle non reconnu.", role });
        }
    }
}
