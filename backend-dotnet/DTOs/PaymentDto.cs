using System;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO renvoyant les informations détaillées d'un paiement.
    /// </summary>
    public class PaymentDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
