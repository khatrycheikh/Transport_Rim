using System;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO renvoyant les détails d'un ticket de transport avec les informations du trajet et du passager.
    /// </summary>
    public class TicketDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string QrCodeData { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }

        // Détails du trajet
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public int ReservedSeats { get; set; }
        public decimal TotalPrice { get; set; }

        // Détails du voyageur
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
    }
}
