using System;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente le ticket de transport généré suite à la confirmation d'une réservation.
    /// </summary>
    public class Ticket
    {
        public int Id { get; set; }
        
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public string QrCodeData { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
