using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la mise à jour du statut d'une réservation par l'administrateur (ex: validation).
    /// </summary>
    public class UpdateReservationStatusDto
    {
        [Required(ErrorMessage = "Le statut est obligatoire.")]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Le statut doit être 'Pending', 'Confirmed' ou 'Cancelled'.")]
        public string Status { get; set; } = string.Empty;
    }
}
