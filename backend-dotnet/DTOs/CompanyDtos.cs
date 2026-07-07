using System;
using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant les détails d'une compagnie de transport.
    /// </summary>
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO pour la création d'une compagnie.
    /// </summary>
    public class CreateCompanyDto
    {
        [Required(ErrorMessage = "Le nom de la compagnie est obligatoire.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Le nom doit contenir entre 3 et 100 caractères.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        [Phone(ErrorMessage = "Le format du numéro de téléphone est invalide.")]
        [StringLength(20, ErrorMessage = "Le numéro de téléphone ne doit pas dépasser 20 caractères.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
        [StringLength(100, ErrorMessage = "L'adresse email ne doit pas dépasser 100 caractères.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse de la compagnie est obligatoire.")]
        [StringLength(200, ErrorMessage = "L'adresse ne doit pas dépasser 200 caractères.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du responsable (Company Admin) est obligatoire.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères.")]
        public string AdminName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone du responsable est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le numéro de téléphone ne doit pas dépasser 20 caractères.")]
        public string AdminPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe initial du responsable est obligatoire.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string AdminPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO pour la mise à jour du statut d'une compagnie par l'administrateur.
    /// </summary>
    public class UpdateCompanyStatusDto
    {
        [Required(ErrorMessage = "Le statut est obligatoire.")]
        [RegularExpression("^(Pending|Active|Suspended)$", ErrorMessage = "Le statut doit être 'Pending', 'Active' ou 'Suspended'.")]
        public string Status { get; set; } = string.Empty;
    }
}
