// Services/AIService.cs
using System.Net.Http.Headers;
using System.Text.Json;
using APM.API.Data;
using APM.API.DTOs.Chat;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public interface IAIService
    {
        Task<string> AskAsync(string userMessage, string systemPrompt);
        Task<string> SuggestActionsAsync(int planId);
        Task<string> SummarizePlanAsync(int planId);
    }

    public class AIService : IAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AIService> _logger;

        public AIService(
            IHttpClientFactory httpClientFactory,
            AppDbContext db,
            IConfiguration config,
            ILogger<AIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
            _config = config;
            _logger = logger;
        }

        // Méthode de base : envoyer un message à Gemini
        public async Task<string> AskAsync(string userMessage, string systemPrompt)
        {
            var apiKey = _config["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini:ApiKey manquante.");
            var model = _config["Gemini:Model"] ?? "gemini-2.0-flash";

            var payload = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = userMessage.Trim() } }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 2048
                }
            };

            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = JsonContent.Create(payload);

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec appel HTTP vers Gemini.");
                throw new HttpRequestException("Impossible de joindre Gemini.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Gemini API {Status}: {Body}", response.StatusCode, err);
                throw new HttpRequestException($"Gemini a retourné {response.StatusCode}: {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            // Extraction du texte de la réponse Gemini
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? throw new InvalidOperationException("Réponse Gemini vide.");
        }

        // Fonctionnalité 1 : suggestions d'actions pour un plan
        public async Task<string> SuggestActionsAsync(int planId)
        {
            var plan = await _db.ActionPlans
                .AsNoTracking()
                .Include(p => p.Actions)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == planId)
                ?? throw new KeyNotFoundException($"Plan {planId} introuvable.");

            // Récupère l'historique des actions efficaces similaires
            var historique = await _db.ActionItems
                .AsNoTracking()
                .Where(a => a.Status == "Clôturé"
                         && a.Effectiveness == "Efficace"
                         && a.Theme == plan.Title)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => $"- Thème: {a.Theme} | Action: {a.ActionDescription} | Méthode: {a.RealizationMethod}")
                .ToListAsync();

            var contexteHistorique = historique.Any()
                ? string.Join("\n", historique)
                : "Aucun historique disponible pour ce type de problème.";

            var systemPrompt = """
                Tu es un expert en amélioration continue et qualité industrielle
                dans une usine de fabrication électronique (TIS Circuits).
                Tu suis la méthode PDCA (Plan-Do-Check-Act).
                Réponds toujours en français. Sois concis, pratique et actionnable.
                """;

            var userMessage = $"""
                Voici un plan d'action à traiter :
                - Titre : {plan.Title}
                - Description : {plan.Description}
                - Priorité : {plan.Priority}
                - Département : {plan.Department?.Name ?? "Non spécifié"}

                Actions similaires résolues avec succès par le passé :
                {contexteHistorique}

                Suggère 3 actions correctives concrètes et rapides pour résoudre ce problème.
                Format : liste numérotée, une action par ligne, maximum 2 phrases chacune.
                """;

            return await AskAsync(userMessage, systemPrompt);
        }

        // Fonctionnalité 2 : résumé automatique de clôture
        public async Task<string> SummarizePlanAsync(int planId)
        {
            var plan = await _db.ActionPlans
                .AsNoTracking()
                .Include(p => p.Actions)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == planId)
                ?? throw new KeyNotFoundException($"Plan {planId} introuvable.");

            var actions = plan.Actions
                .Select(a => $"- [{a.Status}] {a.Theme}: {a.RealizationMethod ?? "Non renseigné"}")
                .ToList();

            var systemPrompt = """
                Tu es un rédacteur professionnel spécialisé dans les rapports qualité industriels.
                Réponds toujours en français. Ton professionnel et synthétique.
                """;

            var userMessage = $"""
                Rédige un résumé de clôture professionnel pour ce plan d'action PDCA :

                Titre : {plan.Title}
                Objectif : {plan.Description}
                Département : {plan.Department?.Name ?? "Non spécifié"}
                Statut final : {plan.Status}

                Actions réalisées :
                {string.Join("\n", actions)}

                Format : un paragraphe de 5 à 6 lignes.
                Mentionne les résultats obtenus, les méthodes utilisées, et les enseignements tirés.
                """;

            return await AskAsync(userMessage, systemPrompt);
        }
    }
}
