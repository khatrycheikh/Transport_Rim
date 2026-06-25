using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs.Auth
{
    /// <summary>
    /// DTO pour la requête de connexion d'un utilisateur.
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        public string Password { get; set; } = string.Empty;
    }
}
