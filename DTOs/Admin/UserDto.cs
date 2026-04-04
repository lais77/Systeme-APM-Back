namespace APM.API.DTOs.Admin
{
    public class UserDto
    {
        public int id { get; set; }
        public string nom { get; set; } = string.Empty;
        public string prenom { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public bool actif { get; set; }
        public string? departement { get; set; }
        public string? equipe { get; set; }
        public string? chef { get; set; }
        public DateTime dateCreation { get; set; }
        public DateTime? dernierLogin { get; set; }
    }

    public class CreateUserDto
    {
        public string nom { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public int? departementId { get; set; }
        public int? equipeId { get; set; }
        public int? chefId { get; set; }
    }

    public class UpdateUserDto
    {
        public string nom { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public int? departementId { get; set; }
        public int? equipeId { get; set; }
        public int? chefId { get; set; }
        public bool actif { get; set; }
    }
}