using System.Collections.Generic;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant l'état d'un siège précis sur un trajet.
    /// </summary>
    public class SeatDto
    {
        public int SeatNumber { get; set; }

        /// <summary>
        /// "Available", "Pending" ou "Confirmed".
        /// </summary>
        public string Status { get; set; } = "Available";
    }

    /// <summary>
    /// DTO représentant la carte complète des sièges d'un trajet.
    /// </summary>
    public class SeatMapDto
    {
        public int Capacity { get; set; }
        public List<SeatDto> Seats { get; set; } = new();
    }
}
