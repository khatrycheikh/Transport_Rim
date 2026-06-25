using System;
using System.Collections.Generic;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente un utilisateur inscrit sur la plateforme de transport.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Traveler;

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        // Relation un-à-plusieurs (Un voyageur peut avoir plusieurs réservations)
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
