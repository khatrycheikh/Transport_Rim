using System;
using System.Collections.Generic;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente une compagnie de transport partenaire enregistrée sur la plateforme.
    /// </summary>
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Active, Suspended
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relation un-à-plusieurs (Une compagnie possède plusieurs bus)
        public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }
}
