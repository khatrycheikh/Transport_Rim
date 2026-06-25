using System;
using System.Collections.Generic;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente un bus affecté à des trajets pour le compte d'une compagnie.
    /// </summary>
    public class Bus
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }

        // Relation un-à-plusieurs (Un bus effectue plusieurs trajets)
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
