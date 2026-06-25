using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransportRim.Api.DTOs;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Service gérant la planification, la recherche et l'administration des trajets de transport.
    /// </summary>
    public interface ITripService
    {
        /// <summary>
        /// Recherche les trajets disponibles selon la ville de départ, d'arrivée et la date (Recherche publique).
        /// </summary>
        Task<IEnumerable<TripDto>> SearchTripsAsync(string? departureCity, string? arrivalCity, DateTime? date);

        /// <summary>
        /// Récupère les détails d'un trajet spécifique.
        /// </summary>
        Task<TripDto?> GetTripByIdAsync(int id);

        /// <summary>
        /// Planifie un nouveau trajet (Réservé aux compagnies).
        /// </summary>
        Task<TripDto> CreateTripAsync(CreateTripDto dto, int companyId);

        /// <summary>
        /// Récupère la liste de tous les trajets planifiés par une compagnie spécifique.
        /// </summary>
        Task<IEnumerable<TripDto>> GetTripsByCompanyIdAsync(int companyId);

        /// <summary>
        /// Supprime un trajet sous condition de propriété de la compagnie.
        /// </summary>
        Task<bool> DeleteTripAsync(int id, int companyId);
    }
}
