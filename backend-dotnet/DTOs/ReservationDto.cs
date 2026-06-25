using System;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant les détails d'une réservation.
    /// </summary>
    public class ReservationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public decimal TripPrice { get; set; }
        public int ReservedSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
