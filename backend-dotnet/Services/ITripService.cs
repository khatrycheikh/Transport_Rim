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
        /// Planifie un nouveau trajet pour le compte d'une compagnie (le bus doit appartenir à `companyId`).
        /// </summary>
        Task<TripDto> CreateTripAsync(CreateTripDto dto, int companyId);

        /// <summary>
        /// Récupère la liste de tous les trajets planifiés par une compagnie spécifique.
        /// </summary>
        Task<IEnumerable<TripDto>> GetTripsByCompanyIdAsync(int companyId);

        /// <summary>
        /// Récupère la liste de tous les trajets, toutes compagnies confondues (Admin uniquement, lecture seule).
        /// </summary>
        Task<IEnumerable<TripDto>> GetAllTripsAsync();

        /// <summary>
        /// Met à jour un trajet sous condition de propriété de la compagnie.
        /// </summary>
        Task<TripDto?> UpdateTripAsync(int id, UpdateTripDto dto, int companyId);

        /// <summary>
        /// Supprime un trajet sous condition de propriété de la compagnie.
        /// </summary>
        Task<bool> DeleteTripAsync(int id, int companyId);

        /// <summary>
        /// Récupère la carte des sièges (disponible / en attente / confirmé) d'un trajet.
        /// </summary>
        Task<SeatMapDto?> GetSeatMapAsync(int tripId);
    }
}
