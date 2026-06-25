using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la mise à jour des informations de profil utilisateur.
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le numéro de téléphone ne doit pas dépasser 20 caractères.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
