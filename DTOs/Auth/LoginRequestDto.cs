using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs.Auth
{
    /// <summary>
    /// DTO pour la requête de connexion d'un utilisateur.
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        public string Password { get; set; } = string.Empty;
    }
}
