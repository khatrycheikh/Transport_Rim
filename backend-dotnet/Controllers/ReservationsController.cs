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
        /// Récupère la liste des réservations : toutes pour un Admin, celles de sa compagnie pour un compte Company.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Company")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(System.Collections.Generic.IEnumerable<ReservationDto>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                    .ThenInclude(t => t!.Bus)
                .Include(r => r.Payment)
                .Include(r => r.Seats)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var companyId = GetUserCompanyId();
                if (companyId == null)
                {
                    return Forbid();
                }

                query = query.Where(r => r.Trip!.Bus!.CompanyId == companyId.Value);
            }

            var reservations = await query.ToListAsync();

            var dtos = reservations.Select(r => MapToDto(r));

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
                    .ThenInclude(t => t!.Bus)
                .Include(r => r.Payment)
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation demandée n'existe pas." });
            }

            // Un utilisateur ne peut consulter que ses propres réservations, sauf s'il est Admin
            // ou une Company dont la réservation concerne un trajet de sa compagnie.
            bool isAuthorized = reservation.UserId == userId || User.IsInRole("Admin");

            if (!isAuthorized && User.IsInRole("Company"))
            {
                var companyId = GetUserCompanyId();
                isAuthorized = companyId != null && reservation.Trip?.Bus?.CompanyId == companyId.Value;
            }

            if (!isAuthorized)
            {
                return Forbid();
            }

            return Ok(MapToDto(reservation));
        }

        /// <summary>
        /// Crée une nouvelle réservation pour un trajet, pour des sièges précis.
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

            // 1. Récupération du trajet et de son bus (pour connaître la capacité)
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.Id == request.TripId);
            if (trip == null)
            {
                return NotFound(new { message = "Le trajet spécifié n'existe pas." });
            }

            var seatValidation = ValidateSeatNumbers(request.SeatNumbers, trip.Bus?.Capacity ?? 0);
            if (seatValidation != null)
            {
                return BadRequest(new { message = seatValidation });
            }

            var seatCount = request.SeatNumbers.Count;

            // 2. Vérification de la disponibilité des places
            if (trip.AvailableSeats < seatCount)
            {
                return BadRequest(new { message = $"Nombre de places disponibles insuffisant. Places restantes : {trip.AvailableSeats}." });
            }

            // 3. Vérification qu'aucun des sièges demandés n'est déjà occupé (Pending ou Confirmed)
            var alreadyTaken = await _context.ReservationSeats
                .AnyAsync(rs => rs.TripId == trip.Id && request.SeatNumbers.Contains(rs.SeatNumber));
            if (alreadyTaken)
            {
                return BadRequest(new { message = "Un ou plusieurs sièges sélectionnés sont déjà réservés. Veuillez en choisir d'autres." });
            }

            // 4. Déduction automatique des places réservées
            trip.AvailableSeats -= seatCount;

            // 5. Calcul du prix total
            var totalPrice = seatCount * trip.Price;

            // 6. Instanciation de la réservation et de ses sièges
            var reservation = new Reservation
            {
                UserId = userId,
                TripId = request.TripId,
                ReservedSeats = seatCount,
                TotalPrice = totalPrice,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Seats = request.SeatNumbers
                    .Select(seatNumber => new ReservationSeat { TripId = request.TripId, SeatNumber = seatNumber })
                    .ToList()
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Charger l'utilisateur pour renvoyer le profil complet dans le DTO
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var dto = MapToDto(reservation, user, trip);

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, dto);
        }

        /// <summary>
        /// Modifie les sièges d'une réservation existante.
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
                    .ThenInclude(t => t!.Bus)
                .Include(r => r.Seats)
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

            var seatValidation = ValidateSeatNumbers(request.SeatNumbers, trip.Bus?.Capacity ?? 0);
            if (seatValidation != null)
            {
                return BadRequest(new { message = seatValidation });
            }

            var newSeatCount = request.SeatNumbers.Count;

            // Vérifier que les nouveaux sièges ne sont pas occupés par une AUTRE réservation
            var alreadyTaken = await _context.ReservationSeats
                .AnyAsync(rs => rs.TripId == trip.Id && rs.ReservationId != id && request.SeatNumbers.Contains(rs.SeatNumber));
            if (alreadyTaken)
            {
                return BadRequest(new { message = "Un ou plusieurs sièges sélectionnés sont déjà réservés. Veuillez en choisir d'autres." });
            }

            // Calcul de la différence de places
            var seatDifference = newSeatCount - reservation.ReservedSeats;

            // Si on augmente la réservation, on vérifie la disponibilité
            if (seatDifference > 0 && trip.AvailableSeats < seatDifference)
            {
                return BadRequest(new { message = $"Nombre de places disponibles insuffisant pour cette modification. Places restantes dans le trajet : {trip.AvailableSeats}." });
            }

            // Ajustement automatique des places disponibles du trajet
            trip.AvailableSeats -= seatDifference;

            // Remplacement des sièges occupés par cette réservation
            _context.ReservationSeats.RemoveRange(reservation.Seats);
            reservation.Seats = request.SeatNumbers
                .Select(seatNumber => new ReservationSeat { TripId = trip.Id, SeatNumber = seatNumber })
                .ToList();

            // Mise à jour de la réservation
            reservation.ReservedSeats = newSeatCount;
            reservation.TotalPrice = newSeatCount * trip.Price;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(reservation));
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
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation demandée n'existe pas." });
            }

            reservation.Status = Enum.Parse<ReservationStatus>(request.Status);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(reservation));
        }

        /// <summary>
        /// Supprime une réservation, libère ses sièges et restitue les places au trajet associé.
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

            // La suppression de la réservation supprime en cascade ses ReservationSeat (siège libéré).
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

        /// <summary>
        /// Valide la liste de numéros de sièges demandés : pas de doublon, au moins un, tous dans la capacité du bus.
        /// Retourne un message d'erreur, ou null si la liste est valide.
        /// </summary>
        private static string? ValidateSeatNumbers(System.Collections.Generic.List<int> seatNumbers, int busCapacity)
        {
            if (seatNumbers.Distinct().Count() != seatNumbers.Count)
            {
                return "La liste de sièges contient des doublons.";
            }

            if (busCapacity <= 0 || seatNumbers.Any(s => s < 1 || s > busCapacity))
            {
                return $"Un ou plusieurs numéros de siège sont invalides pour ce bus (capacité : {busCapacity}).";
            }

            return null;
        }

        private static ReservationDto MapToDto(Reservation reservation, User? user = null, Trip? trip = null)
        {
            var resolvedTrip = trip ?? reservation.Trip;
            var resolvedUser = user ?? reservation.User;

            return new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = resolvedUser?.Name ?? string.Empty,
                TripId = reservation.TripId,
                DepartureCity = resolvedTrip?.DepartureCity ?? string.Empty,
                ArrivalCity = resolvedTrip?.ArrivalCity ?? string.Empty,
                TripDate = resolvedTrip?.Date ?? DateTime.MinValue,
                TripPrice = resolvedTrip?.Price ?? 0,
                ReservedSeats = reservation.ReservedSeats,
                SeatNumbers = reservation.Seats.Select(s => s.SeatNumber).OrderBy(n => n).ToList(),
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                CreatedAt = reservation.CreatedAt,
                PaymentId = reservation.Payment?.Id,
                PaymentMethod = reservation.Payment?.Method.ToString(),
                PaymentStatus = reservation.Payment?.Status,
                PaymentTransactionId = reservation.Payment?.TransactionId
            };
        }

        private int? GetUserCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
