using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la modification d'une réservation existante.
    /// </summary>
    public class UpdateReservationDto
    {
        [Required(ErrorMessage = "Les numéros de sièges sont obligatoires.")]
        [MinLength(1, ErrorMessage = "Vous devez sélectionner au moins un siège.")]
        [MaxLength(10, ErrorMessage = "Vous ne pouvez pas réserver plus de 10 sièges.")]
        public List<int> SeatNumbers { get; set; } = new();
    }
}
