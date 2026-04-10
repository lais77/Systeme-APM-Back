using System.Text.Json.Serialization;

namespace APM.API.DTOs.Chat
{
    public class ClaudeApiRequestDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("system")]
        public string System { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<ClaudeUserMessageDto> Messages { get; set; } = new();
    }

    public class ClaudeUserMessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class ClaudeMessageResponseDto
    {
        [JsonPropertyName("content")]
        public List<ClaudeContentBlockDto>? Content { get; set; }
    }

    public class ClaudeContentBlockDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
