using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la demande de changement de mot de passe utilisateur.
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Le mot de passe actuel est obligatoire.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 6 caractères.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
