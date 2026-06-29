namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant un utilisateur de la plateforme (vue Admin).
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }
}
