using System;
using System.Collections.Generic;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente un trajet de voyage interurbain planifié.
    /// </summary>
    public class Trip
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public Bus? Bus { get; set; }
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }

        // Relation un-à-plusieurs (Un trajet possède plusieurs réservations)
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
