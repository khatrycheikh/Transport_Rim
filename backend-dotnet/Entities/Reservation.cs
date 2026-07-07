using System;
using System.Collections.Generic;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente une réservation effectuée par un voyageur pour un trajet spécifique.
    /// </summary>
    public class Reservation
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }

        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        public int ReservedSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relation un-à-un (Une réservation peut avoir un paiement et un ticket)
        public Payment? Payment { get; set; }
        public Ticket? Ticket { get; set; }

        // Relation un-à-plusieurs (Une réservation occupe un ou plusieurs sièges précis)
        public ICollection<ReservationSeat> Seats { get; set; } = new List<ReservationSeat>();
    }
}
