using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant les détails d'un bus.
    /// </summary>
    public class BusDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BusNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }

    /// <summary>
    /// DTO pour la création d'un bus.
    /// </summary>
    public class CreateBusDto
    {
        [Required(ErrorMessage = "Le numéro d'immatriculation du bus est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le numéro d'immatriculation ne doit pas dépasser 20 caractères.")]
        public string BusNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La capacité du bus est obligatoire.")]
        [Range(10, 100, ErrorMessage = "La capacité du bus doit être comprise entre 10 et 100 places.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Compagnie propriétaire du bus. Obligatoire uniquement lorsque la requête est faite par un Administrateur
        /// (pour un compte Company, la compagnie est déduite automatiquement du jeton).
        /// </summary>
        public int? CompanyId { get; set; }
    }

    /// <summary>
    /// DTO pour la modification d'un bus.
    /// </summary>
    public class UpdateBusDto
    {
        [Required(ErrorMessage = "Le numéro d'immatriculation du bus est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le numéro d'immatriculation ne doit pas dépasser 20 caractères.")]
        public string BusNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La capacité du bus est obligatoire.")]
        [Range(10, 100, ErrorMessage = "La capacité du bus doit être comprise entre 10 et 100 places.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Nouvelle compagnie propriétaire du bus. Pris en compte uniquement lorsque la requête est faite
        /// par un Administrateur (un compte Company ne peut pas réassigner son bus à une autre compagnie).
        /// </summary>
        public int? CompanyId { get; set; }
    }
}
