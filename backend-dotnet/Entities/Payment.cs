using System;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente un paiement associé à une réservation de trajet.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }
        
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
