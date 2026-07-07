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
    /// Implémentation du service gérant les bus d'une compagnie.
    /// </summary>
    public class BusService : IBusService
    {
        private readonly TransportRimDbContext _context;

        public BusService(TransportRimDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusDto>> GetBusesByCompanyIdAsync(int companyId)
        {
            var buses = await _context.Buses
                .Include(b => b.Company)
                .Where(b => b.CompanyId == companyId)
                .ToListAsync();

            return buses.Select(b => MapToDto(b));
        }

        public async Task<BusDto?> GetBusByIdAsync(int id, int companyId)
        {
            var bus = await _context.Buses
                .Include(b => b.Company)
                .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId);

            return bus == null ? null : MapToDto(bus);
        }

        public async Task<BusDto> CreateBusAsync(CreateBusDto dto, int companyId)
        {
            var busNumberNormalized = dto.BusNumber.Trim().ToUpperInvariant();

            // Vérifier si un bus avec la même immatriculation existe déjà
            var exists = await _context.Buses.AnyAsync(b => b.BusNumber == busNumberNormalized);
            if (exists)
            {
                throw new InvalidOperationException("Un bus avec ce numéro d'immatriculation existe déjà dans le système.");
            }

            var bus = new Bus
            {
                CompanyId = companyId,
                BusNumber = busNumberNormalized,
                Capacity = dto.Capacity
            };

            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();

            // Recharge pour charger la relation compagnie
            var createdBus = await _context.Buses
                .Include(b => b.Company)
                .FirstAsync(b => b.Id == bus.Id);

            return MapToDto(createdBus);
        }

        public async Task<BusDto?> UpdateBusAsync(int id, UpdateBusDto dto, int companyId)
        {
            var bus = await _context.Buses
                .Include(b => b.Company)
                .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId);

            if (bus == null)
            {
                return null;
            }

            var busNumberNormalized = dto.BusNumber.Trim().ToUpperInvariant();

            // Vérifier si un autre bus possède déjà cette immatriculation
            var exists = await _context.Buses.AnyAsync(b => b.Id != id && b.BusNumber == busNumberNormalized);
            if (exists)
            {
                throw new InvalidOperationException("Un autre bus possède déjà ce numéro d'immatriculation dans le système.");
            }

            bus.BusNumber = busNumberNormalized;
            bus.Capacity = dto.Capacity;

            await _context.SaveChangesAsync();
            return MapToDto(bus);
        }

        public async Task<bool> DeleteBusAsync(int id, int companyId)
        {
            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId);

            if (bus == null)
            {
                return false;
            }

            await EnsureNoTripsAsync(id);

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task EnsureNoTripsAsync(int busId)
        {
            var hasTrips = await _context.Trips.AnyAsync(t => t.BusId == busId);
            if (hasTrips)
            {
                throw new InvalidOperationException(
                    "Impossible de supprimer ce bus <-> des trajets lui sont encore associés. Supprimez d'abord ses trajets.");
            }
        }

        private static BusDto MapToDto(Bus bus)
        {
            return new BusDto
            {
                Id = bus.Id,
                CompanyId = bus.CompanyId,
                CompanyName = bus.Company?.Name ?? string.Empty,
                BusNumber = bus.BusNumber,
                Capacity = bus.Capacity
            };
        }
    }
}
