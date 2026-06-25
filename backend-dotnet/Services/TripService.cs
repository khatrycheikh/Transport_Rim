using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Data;
using TransportRim.Api.DTOs;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Implémentation du service gérant les trajets.
    /// </summary>
    public class TripService : ITripService
    {
        private readonly TransportRimDbContext _context;

        public TripService(TransportRimDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TripDto>> SearchTripsAsync(string? departureCity, string? arrivalCity, DateTime? date)
        {
            var query = _context.Trips
                .Include(t => t.Bus)
                .ThenInclude(b => b!.Company)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(departureCity))
            {
                var depLower = departureCity.Trim().ToLowerInvariant();
                query = query.Where(t => t.DepartureCity.ToLower() == depLower);
            }

            if (!string.IsNullOrWhiteSpace(arrivalCity))
            {
                var arrLower = arrivalCity.Trim().ToLowerInvariant();
                query = query.Where(t => t.ArrivalCity.ToLower() == arrLower);
            }

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(t => t.Date.Date == targetDate);
            }

            // Uniquement afficher les trajets d'une compagnie active
            query = query.Where(t => t.Bus!.Company!.Status == "Active");

            var trips = await query.ToListAsync();
            return trips.Select(t => MapToDto(t));
        }

        public async Task<TripDto?> GetTripByIdAsync(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .ThenInclude(b => b!.Company)
                .FirstOrDefaultAsync(t => t.Id == id);

            return trip == null ? null : MapToDto(trip);
        }

        public async Task<TripDto> CreateTripAsync(CreateTripDto dto, int companyId)
        {
            // Vérification de sécurité supplémentaire : s'assurer que le bus appartient à la compagnie de l'utilisateur connecté
            var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == dto.BusId && b.CompanyId == companyId);
            if (bus == null)
            {
                throw new InvalidOperationException("Le bus sélectionné n'existe pas ou n'appartient pas à votre compagnie.");
            }

            // Vérifier si le bus est déjà affecté à un autre trajet à la même date et heure
            var busBusy = await _context.Trips.AnyAsync(t => t.BusId == dto.BusId && t.Date == dto.Date.Date && t.Time == dto.Time);
            if (busBusy)
            {
                throw new InvalidOperationException("Ce bus est déjà affecté à un autre trajet à la même date et heure.");
            }

            var trip = new Trip
            {
                BusId = dto.BusId,
                DepartureCity = dto.DepartureCity.Trim(),
                ArrivalCity = dto.ArrivalCity.Trim(),
                Date = dto.Date.Date,
                Time = dto.Time,
                Price = dto.Price,
                AvailableSeats = dto.AvailableSeats
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            // Charge les relations pour renvoyer le DTO complet
            var createdTrip = await _context.Trips
                .Include(t => t.Bus)
                .ThenInclude(b => b!.Company)
                .FirstAsync(t => t.Id == trip.Id);

            return MapToDto(createdTrip);
        }

        public async Task<IEnumerable<TripDto>> GetTripsByCompanyIdAsync(int companyId)
        {
            var trips = await _context.Trips
                .Include(t => t.Bus)
                .ThenInclude(b => b!.Company)
                .Where(t => t.Bus!.CompanyId == companyId)
                .ToListAsync();

            return trips.Select(t => MapToDto(t));
        }

        public async Task<bool> DeleteTripAsync(int id, int companyId)
        {
            // Uniquement supprimable par la compagnie propriétaire
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.Id == id && t.Bus!.CompanyId == companyId);

            if (trip == null)
            {
                return false;
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TripDto MapToDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                BusId = trip.BusId,
                BusNumber = trip.Bus?.BusNumber ?? string.Empty,
                CompanyId = trip.Bus?.CompanyId ?? 0,
                CompanyName = trip.Bus?.Company?.Name ?? string.Empty,
                DepartureCity = trip.DepartureCity,
                ArrivalCity = trip.ArrivalCity,
                Date = trip.Date,
                Time = trip.Time,
                Price = trip.Price,
                AvailableSeats = trip.AvailableSeats
            };
        }
    }
}
