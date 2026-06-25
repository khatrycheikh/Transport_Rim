using System;
using System.ComponentModel.DataAnnotations;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant les détails d'un trajet.
    /// </summary>
    public class TripDto
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
    }

    /// <summary>
    /// DTO pour la planification d'un nouveau trajet par une compagnie.
    /// </summary>
    public class CreateTripDto
    {
        [Required(ErrorMessage = "Le bus est obligatoire.")]
        public int BusId { get; set; }

        [Required(ErrorMessage = "La ville de départ est obligatoire.")]
        [StringLength(50, ErrorMessage = "La ville de départ ne doit pas dépasser 50 caractères.")]
        public string DepartureCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville d'arrivée est obligatoire.")]
        [StringLength(50, ErrorMessage = "La ville d'arrivée ne doit pas dépasser 50 caractères.")]
        public string ArrivalCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date du trajet est obligatoire.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "L'heure du trajet est obligatoire.")]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Le prix du trajet est obligatoire.")]
        [Range(50, 10000, ErrorMessage = "Le prix doit être compris entre 50 MRU et 10 000 MRU.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Le nombre de places disponibles est obligatoire.")]
        [Range(10, 100, ErrorMessage = "Le nombre de places doit être compris entre 10 et 100.")]
        public int AvailableSeats { get; set; }
    }
}
