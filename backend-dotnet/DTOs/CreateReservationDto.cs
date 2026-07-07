using System.Collections.Generic;
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

        [Required(ErrorMessage = "Les numéros de sièges sont obligatoires.")]
        [MinLength(1, ErrorMessage = "Vous devez sélectionner au moins un siège.")]
        [MaxLength(10, ErrorMessage = "Vous ne pouvez pas réserver plus de 10 sièges.")]
        public List<int> SeatNumbers { get; set; } = new();
    }
}
