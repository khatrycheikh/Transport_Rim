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
    /// Contrôleur gérant la consultation des tickets de transport générés.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly TransportRimDbContext _context;

        public TicketsController(TransportRimDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les détails d'un ticket spécifique par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TicketDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var ticket = await _context.Tickets
                .Include(tk => tk.Reservation)
                    .ThenInclude(r => r!.User)
                .Include(tk => tk.Reservation)
                    .ThenInclude(r => r!.Trip)
                        .ThenInclude(t => t!.Bus)
                .FirstOrDefaultAsync(tk => tk.Id == id);

            if (ticket == null)
            {
                return NotFound(new { message = "Le ticket demandé n'existe pas." });
            }

            // Sécurité :
            // - Si Admin : accès total
            // - Si Voyageur : doit être le propriétaire du ticket (via UserId de la réservation)
            // - Si Compagnie : doit gérer le bus réalisant le trajet associé
            bool isAuthorized = User.IsInRole("Admin");

            if (!isAuthorized && User.IsInRole("Traveler"))
            {
                isAuthorized = ticket.Reservation?.UserId == userId;
            }

            if (!isAuthorized && User.IsInRole("Company"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out var companyId))
                {
                    isAuthorized = ticket.Reservation?.Trip?.Bus?.CompanyId == companyId;
                }
            }

            if (!isAuthorized)
            {
                return Forbid();
            }

            var dto = new TicketDto
            {
                Id = ticket.Id,
                ReservationId = ticket.ReservationId,
                QrCodeData = ticket.QrCodeData,
                GeneratedAt = ticket.GeneratedAt,
                DepartureCity = ticket.Reservation?.Trip?.DepartureCity ?? string.Empty,
                ArrivalCity = ticket.Reservation?.Trip?.ArrivalCity ?? string.Empty,
                TripDate = ticket.Reservation?.Trip?.Date ?? DateTime.MinValue,
                ReservedSeats = ticket.Reservation?.ReservedSeats ?? 0,
                TotalPrice = ticket.Reservation?.TotalPrice ?? 0,
                PassengerName = ticket.Reservation?.User?.Name ?? string.Empty,
                PassengerPhone = ticket.Reservation?.User?.PhoneNumber ?? string.Empty
            };

            return Ok(dto);
        }

        /// <summary>
        /// Récupère le ticket associé à une réservation spécifique.
        /// </summary>
        [HttpGet("reservation/{reservationId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TicketDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByReservationId(int reservationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var ticket = await _context.Tickets
                .Include(tk => tk.Reservation)
                    .ThenInclude(r => r!.User)
                .Include(tk => tk.Reservation)
                    .ThenInclude(r => r!.Trip)
                        .ThenInclude(t => t!.Bus)
                .FirstOrDefaultAsync(tk => tk.ReservationId == reservationId);

            if (ticket == null)
            {
                return NotFound(new { message = "Aucun ticket généré pour cette réservation." });
            }

            // Sécurité identique
            bool isAuthorized = User.IsInRole("Admin");

            if (!isAuthorized && User.IsInRole("Traveler"))
            {
                isAuthorized = ticket.Reservation?.UserId == userId;
            }

            if (!isAuthorized && User.IsInRole("Company"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out var companyId))
                {
                    isAuthorized = ticket.Reservation?.Trip?.Bus?.CompanyId == companyId;
                }
            }

            if (!isAuthorized)
            {
                return Forbid();
            }

            var dto = new TicketDto
            {
                Id = ticket.Id,
                ReservationId = ticket.ReservationId,
                QrCodeData = ticket.QrCodeData,
                GeneratedAt = ticket.GeneratedAt,
                DepartureCity = ticket.Reservation?.Trip?.DepartureCity ?? string.Empty,
                ArrivalCity = ticket.Reservation?.Trip?.ArrivalCity ?? string.Empty,
                TripDate = ticket.Reservation?.Trip?.Date ?? DateTime.MinValue,
                ReservedSeats = ticket.Reservation?.ReservedSeats ?? 0,
                TotalPrice = ticket.Reservation?.TotalPrice ?? 0,
                PassengerName = ticket.Reservation?.User?.Name ?? string.Empty,
                PassengerPhone = ticket.Reservation?.User?.PhoneNumber ?? string.Empty
            };

            return Ok(dto);
        }

        /// <summary>
        /// Liste tous les tickets en fonction des privilèges de l'utilisateur.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TicketDto[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var query = _context.Tickets
                .Include(tk => tk.Reservation)
                    .ThenInclude(r => r!.User)
                .Include(tk => tk.Reservation)
                    .ThenInclude(r => r!.Trip)
                        .ThenInclude(t => t!.Bus)
                .AsQueryable();

            if (User.IsInRole("Traveler"))
            {
                query = query.Where(tk => tk.Reservation!.UserId == userId);
            }
            else if (User.IsInRole("Company"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out var companyId))
                {
                    query = query.Where(tk => tk.Reservation!.Trip!.Bus!.CompanyId == companyId);
                }
                else
                {
                    // Si le gérant n'a pas de compagnie affectée, il ne voit rien
                    return Ok(Array.Empty<TicketDto>());
                }
            }

            var tickets = await query.ToListAsync();

            var dtos = tickets.Select(tk => new TicketDto
            {
                Id = tk.Id,
                ReservationId = tk.ReservationId,
                QrCodeData = tk.QrCodeData,
                GeneratedAt = tk.GeneratedAt,
                DepartureCity = tk.Reservation?.Trip?.DepartureCity ?? string.Empty,
                ArrivalCity = tk.Reservation?.Trip?.ArrivalCity ?? string.Empty,
                TripDate = tk.Reservation?.Trip?.Date ?? DateTime.MinValue,
                ReservedSeats = tk.Reservation?.ReservedSeats ?? 0,
                TotalPrice = tk.Reservation?.TotalPrice ?? 0,
                PassengerName = tk.Reservation?.User?.Name ?? string.Empty,
                PassengerPhone = tk.Reservation?.User?.PhoneNumber ?? string.Empty
            }).ToArray();

            return Ok(dtos);
        }
    }
}
