using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la modification d'une réservation existante.
    /// </summary>
    public class UpdateReservationDto
    {
        [Required(ErrorMessage = "Le nombre de places réservées est obligatoire.")]
        [Range(1, 10, ErrorMessage = "Vous devez réserver entre 1 et 10 places.")]
        public int ReservedSeats { get; set; }
    }
}
