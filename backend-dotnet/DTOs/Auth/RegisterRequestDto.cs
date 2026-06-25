using System.ComponentModel.DataAnnotations;
using TransportRim.Api.Entities;

namespace TransportRim.Api.DTOs.Auth
{
    /// <summary>
    /// DTO pour la requête d'inscription d'un nouvel utilisateur.
    /// </summary>
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le numéro de téléphone ne doit pas dépasser 20 caractères.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Le rôle spécifié est invalide. Les valeurs possibles sont: Admin, Company, Traveler.")]
        public UserRole Role { get; set; } = UserRole.Traveler;

        public int? CompanyId { get; set; }
    }
}
