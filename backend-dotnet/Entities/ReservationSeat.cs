namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente un siège précis occupé par une réservation sur un trajet donné.
    /// L'existence d'une ligne pour (TripId, SeatNumber) signifie que ce siège est indisponible.
    /// </summary>
    public class ReservationSeat
    {
        public int Id { get; set; }

        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        public int SeatNumber { get; set; }
    }
}
