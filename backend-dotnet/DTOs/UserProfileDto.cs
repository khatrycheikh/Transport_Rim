namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant le profil de l'utilisateur avec son nouveau jeton JWT mis à jour.
    /// </summary>
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
