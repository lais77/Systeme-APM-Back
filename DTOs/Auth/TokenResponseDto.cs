namespace APM.API.DTOs.Auth
{
    public class TokenResponseDto
    {
        public string token { get; set; } = string.Empty;
        public DateTime expiration { get; set; }
        public UserProfileDto user { get; set; } = null!;
    }

    public class UserProfileDto
    {
        public int id { get; set; }
        public string fullName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public string? departmentName { get; set; }
    }
}