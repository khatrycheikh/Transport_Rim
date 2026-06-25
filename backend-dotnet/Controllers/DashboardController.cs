using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Data;
using TransportRim.Api.DTOs;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Controllers
{
    /// <summary>
    /// Contrôleur gérant les tableaux de bord et statistiques pour les gérants de compagnies et les administrateurs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly TransportRimDbContext _context;

        public DashboardController(TransportRimDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les statistiques et indicateurs pour la compagnie de transport de l'utilisateur connecté.
        /// </summary>
        [HttpGet("company")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyDashboardDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCompanyDashboard()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(new { message = "L'utilisateur connecté n'est rattaché à aucune compagnie de transport." });
            }

            // 1. Nombre total de bus
            var totalBuses = await _context.Buses
                .CountAsync(b => b.CompanyId == companyId);

            // 2. Nombre total de trajets planifiés
            var totalTrips = await _context.Trips
                .Include(t => t.Bus)
                .CountAsync(t => t.Bus != null && t.Bus.CompanyId == companyId);

            // 3. Récupérer toutes les réservations liées aux trajets de la compagnie
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                    .ThenInclude(t => t!.Bus)
                .Where(r => r.Trip != null && r.Trip.Bus != null && r.Trip.Bus.CompanyId == companyId)
                .ToListAsync();

            var totalReservations = reservations.Count;
            var confirmedReservationsCount = reservations.Count(r => r.Status == ReservationStatus.Confirmed);
            var totalSeatsBooked = reservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.ReservedSeats);
            var totalRevenue = reservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.TotalPrice);

            // 4. Taux de remplissage moyen
            double averageOccupancy = 0;
            var trips = await _context.Trips
                .Include(t => t.Bus)
                .Where(t => t.Bus != null && t.Bus.CompanyId == companyId)
                .ToListAsync();

            if (trips.Count > 0)
            {
                averageOccupancy = trips.Average(t => t.Bus != null && t.Bus.Capacity > 0 
                    ? (double)(t.Bus.Capacity - t.AvailableSeats) / t.Bus.Capacity * 100 
                    : 0);
            }

            // 5. Activités récentes : 5 dernières réservations
            var recentReservations = reservations
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User?.Name ?? string.Empty,
                    TripId = r.TripId,
                    DepartureCity = r.Trip?.DepartureCity ?? string.Empty,
                    ArrivalCity = r.Trip?.ArrivalCity ?? string.Empty,
                    TripDate = r.Trip?.Date ?? DateTime.MinValue,
                    TripPrice = r.Trip?.Price ?? 0,
                    ReservedSeats = r.ReservedSeats,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            // 6. Activités récentes : 5 derniers paiements
            var recentPayments = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r!.Trip)
                        .ThenInclude(t => t!.Bus)
                .Where(p => p.Reservation != null && p.Reservation.Trip != null && p.Reservation.Trip.Bus != null && p.Reservation.Trip.Bus.CompanyId == companyId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    ReservationId = p.ReservationId,
                    Amount = p.Amount,
                    Method = p.Method.ToString(),
                    TransactionId = p.TransactionId,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var dto = new CompanyDashboardDto
            {
                TotalBuses = totalBuses,
                TotalTrips = totalTrips,
                TotalReservations = totalReservations,
                ConfirmedReservationsCount = confirmedReservationsCount,
                TotalSeatsBooked = totalSeatsBooked,
                TotalRevenue = totalRevenue,
                AverageBusOccupancy = Math.Round(averageOccupancy, 2),
                RecentReservations = recentReservations,
                RecentPayments = recentPayments
            };

            return Ok(dto);
        }

        /// <summary>
        /// Récupère les statistiques et indicateurs globaux de la plateforme pour l'Administrateur.
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDashboardDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            // 1. Compagnies
            var companies = await _context.Companies.ToListAsync();
            var totalCompanies = companies.Count;
            var activeCompaniesCount = companies.Count(c => c.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
            var pendingCompaniesCount = companies.Count(c => c.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            var suspendedCompaniesCount = companies.Count(c => c.Status.Equals("Suspended", StringComparison.OrdinalIgnoreCase));

            // 2. Utilisateurs
            var users = await _context.Users.ToListAsync();
            var totalUsersCount = users.Count;
            var travelersCount = users.Count(u => u.Role == UserRole.Traveler);
            var companyManagersCount = users.Count(u => u.Role == UserRole.Company);
            var adminsCount = users.Count(u => u.Role == UserRole.Admin);

            // 3. Système
            var totalBusesCount = await _context.Buses.CountAsync();
            var totalTripsCount = await _context.Trips.CountAsync();

            var reservations = await _context.Reservations.ToListAsync();
            var totalReservationsCount = reservations.Count;
            var globalRevenue = reservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.TotalPrice);

            // 4. Activités récentes : 5 dernières compagnies inscrites
            var recentCompanies = companies
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToList();

            // 5. Activités récentes : 5 derniers paiements globaux
            var recentPayments = await _context.Payments
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    ReservationId = p.ReservationId,
                    Amount = p.Amount,
                    Method = p.Method.ToString(),
                    TransactionId = p.TransactionId,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var dto = new AdminDashboardDto
            {
                TotalCompanies = totalCompanies,
                ActiveCompaniesCount = activeCompaniesCount,
                PendingCompaniesCount = pendingCompaniesCount,
                SuspendedCompaniesCount = suspendedCompaniesCount,
                TotalUsersCount = totalUsersCount,
                TravelersCount = travelersCount,
                CompanyManagersCount = companyManagersCount,
                AdminsCount = adminsCount,
                TotalBusesCount = totalBusesCount,
                TotalTripsCount = totalTripsCount,
                TotalReservationsCount = totalReservationsCount,
                GlobalRevenue = globalRevenue,
                RecentCompanies = recentCompanies,
                RecentPayments = recentPayments
            };

            return Ok(dto);
        }
    }
}
