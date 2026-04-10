using System.ComponentModel.DataAnnotations;

namespace APM.API.DTOs.Chat
{
    public class ChatRequestDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(8000)]
        public string Message { get; set; } = string.Empty;
    }
}
