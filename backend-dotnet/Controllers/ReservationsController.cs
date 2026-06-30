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
using TransportRim.Api.Services;

namespace TransportRim.Api.Controllers
{
    /// <summary>
    /// Contrôleur gérant les réservations de trajets.
    /// Accès réservé aux utilisateurs connectés (Traveler, Company, Admin).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly TransportRimDbContext _context;
        private readonly INotificationService _notificationService;

        public ReservationsController(TransportRimDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Récupère la liste de toutes les réservations, tous voyageurs confondus. (Admin uniquement)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(System.Collections.Generic.IEnumerable<ReservationDto>))]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                .Include(r => r.Payment)
                .ToListAsync();

            var dtos = reservations.Select(reservation => new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name ?? string.Empty,
                TripId = reservation.TripId,
                DepartureCity = reservation.Trip?.DepartureCity ?? string.Empty,
                ArrivalCity = reservation.Trip?.ArrivalCity ?? string.Empty,
                TripDate = reservation.Trip?.Date ?? DateTime.MinValue,
                TripPrice = reservation.Trip?.Price ?? 0,
                ReservedSeats = reservation.ReservedSeats,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt,
                PaymentId = reservation.Payment?.Id,
                PaymentMethod = reservation.Payment?.Method.ToString(),
                PaymentStatus = reservation.Payment?.Status,
                PaymentTransactionId = reservation.Payment?.TransactionId
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Récupère les détails d'une réservation spécifique par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReservationDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation demandée n'existe pas." });
            }

            // Un utilisateur ne peut consulter que ses propres réservations, sauf s'il est Admin
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var dto = new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name ?? string.Empty,
                TripId = reservation.TripId,
                DepartureCity = reservation.Trip?.DepartureCity ?? string.Empty,
                ArrivalCity = reservation.Trip?.ArrivalCity ?? string.Empty,
                TripDate = reservation.Trip?.Date ?? DateTime.MinValue,
                TripPrice = reservation.Trip?.Price ?? 0,
                ReservedSeats = reservation.ReservedSeats,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt,
                PaymentId = reservation.Payment?.Id,
                PaymentMethod = reservation.Payment?.Method.ToString(),
                PaymentStatus = reservation.Payment?.Status,
                PaymentTransactionId = reservation.Payment?.TransactionId
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crée une nouvelle réservation pour un trajet.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReservationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            // 1. Récupération du trajet
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == request.TripId);
            if (trip == null)
            {
                return NotFound(new { message = "Le trajet spécifié n'existe pas." });
            }

            // 2. Vérification de la disponibilité des places
            if (trip.AvailableSeats < request.ReservedSeats)
            {
                return BadRequest(new { message = $"Nombre de places disponibles insuffisant. Places restantes : {trip.AvailableSeats}." });
            }

            // 3. Déduction automatique des places réservées
            trip.AvailableSeats -= request.ReservedSeats;

            // 4. Calcul du prix total
            var totalPrice = request.ReservedSeats * trip.Price;

            // 5. Instanciation de la réservation
            var reservation = new Reservation
            {
                UserId = userId,
                TripId = request.TripId,
                ReservedSeats = request.ReservedSeats,
                TotalPrice = totalPrice,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Charger l'utilisateur pour renvoyer le profil complet dans le DTO
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var dto = new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = user?.Name ?? string.Empty,
                TripId = reservation.TripId,
                DepartureCity = trip.DepartureCity,
                ArrivalCity = trip.ArrivalCity,
                TripDate = trip.Date,
                TripPrice = trip.Price,
                ReservedSeats = reservation.ReservedSeats,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, dto);
        }

        /// <summary>
        /// Modifie le nombre de places d'une réservation existante.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReservationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation demandée n'existe pas." });
            }

            // Un utilisateur ne peut modifier que ses propres réservations, sauf s'il est Admin
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return BadRequest(new { message = "Impossible de modifier une réservation annulée." });
            }

            var trip = reservation.Trip;
            if (trip == null)
            {
                return BadRequest(new { message = "Le trajet associé à cette réservation n'est pas disponible." });
            }

            // Calcul de la différence de places
            var seatDifference = request.ReservedSeats - reservation.ReservedSeats;

            // Si on augmente la réservation, on vérifie la disponibilité
            if (seatDifference > 0 && trip.AvailableSeats < seatDifference)
            {
                return BadRequest(new { message = $"Nombre de places disponibles insuffisant pour cette modification. Places restantes dans le trajet : {trip.AvailableSeats}." });
            }

            // Ajustement automatique des places disponibles du trajet
            trip.AvailableSeats -= seatDifference;

            // Mise à jour de la réservation
            reservation.ReservedSeats = request.ReservedSeats;
            reservation.TotalPrice = request.ReservedSeats * trip.Price;

            await _context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name ?? string.Empty,
                TripId = reservation.TripId,
                DepartureCity = trip.DepartureCity,
                ArrivalCity = trip.ArrivalCity,
                TripDate = trip.Date,
                TripPrice = trip.Price,
                ReservedSeats = reservation.ReservedSeats,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Modifie le statut d'une réservation, ex: la valider (Confirmed). (Admin uniquement)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReservationDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservationStatusDto request)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation demandée n'existe pas." });
            }

            reservation.Status = Enum.Parse<ReservationStatus>(request.Status);
            await _context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name ?? string.Empty,
                TripId = reservation.TripId,
                DepartureCity = reservation.Trip?.DepartureCity ?? string.Empty,
                ArrivalCity = reservation.Trip?.ArrivalCity ?? string.Empty,
                TripDate = reservation.Trip?.Date ?? DateTime.MinValue,
                TripPrice = reservation.Trip?.Price ?? 0,
                ReservedSeats = reservation.ReservedSeats,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt,
                PaymentId = reservation.Payment?.Id,
                PaymentMethod = reservation.Payment?.Method.ToString(),
                PaymentStatus = reservation.Payment?.Status,
                PaymentTransactionId = reservation.Payment?.TransactionId
            };

            return Ok(dto);
        }

        /// <summary>
        /// Supprime une réservation et restitue les places au trajet associé.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation demandée n'existe pas." });
            }

            // Un utilisateur ne peut supprimer que ses propres réservations, sauf s'il est Admin
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var trip = reservation.Trip;
            if (trip != null && reservation.Status != ReservationStatus.Cancelled)
            {
                // Restitution des places réservées au trajet
                trip.AvailableSeats += reservation.ReservedSeats;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            // Envoi de la notification SMS (simulée)
            var travelerName = reservation.User?.Name ?? "Voyageur";
            var travelerPhone = reservation.User?.PhoneNumber ?? string.Empty;
            var departure = reservation.Trip?.DepartureCity ?? string.Empty;
            var arrival = reservation.Trip?.ArrivalCity ?? string.Empty;
            var tripDate = reservation.Trip?.Date.ToString("dd/MM/yyyy") ?? string.Empty;

            var smsMessage = $"Bonjour {travelerName}, votre réservation pour le trajet {departure} -> {arrival} du {tripDate} a bien été annulée et remboursée.";
            await _notificationService.SendNotificationAsync(reservation.UserId, "SMS", travelerPhone, smsMessage);

            return NoContent();
        }
    }
}
