using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la création d'une nouvelle réservation.
    /// </summary>
    public class CreateReservationDto
    {
        [Required(ErrorMessage = "L'identifiant du trajet (TripId) est obligatoire.")]
        public int TripId { get; set; }

        [Required(ErrorMessage = "Le nombre de places à réserver est obligatoire.")]
        [Range(1, 10, ErrorMessage = "Vous devez réserver entre 1 et 10 places.")]
        public int ReservedSeats { get; set; }
    }
}
