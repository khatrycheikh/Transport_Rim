using System;
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
    /// Contrôleur gérant les paiements des réservations de trajets.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly TransportRimDbContext _context;
        private readonly INotificationService _notificationService;

        public PaymentsController(TransportRimDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Crée un paiement pour une réservation existante.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PaymentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            // 1. Récupérer la réservation
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId);

            if (reservation == null)
            {
                return NotFound(new { message = "La réservation spécifiée n'existe pas." });
            }

            // 2. Sécurité : Seul le propriétaire ou un Admin peut payer
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // 3. Validation de l'état de la réservation
            if (reservation.Status == ReservationStatus.Confirmed)
            {
                return BadRequest(new { message = "Cette réservation est déjà confirmée (déjà payée)." });
            }
            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return BadRequest(new { message = "Cette réservation est annulée et ne peut pas être payée." });
            }

            // 4. Validation de l'unicité du code de transaction
            var transactionExists = await _context.Payments.AnyAsync(p => p.TransactionId == request.TransactionId);
            if (transactionExists)
            {
                return BadRequest(new { message = "Ce numéro de transaction a déjà été enregistré dans le système." });
            }

            // 5. Création du paiement (automatiquement Completed dans ce flux de simulation)
            var payment = new Payment
            {
                ReservationId = reservation.Id,
                Amount = reservation.TotalPrice,
                Method = request.Method,
                TransactionId = request.TransactionId,
                Status = "Completed",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // 6. Confirmation automatique de la réservation
            reservation.Status = ReservationStatus.Confirmed;

            // 7. Génération automatique du ticket associé
            var ticket = new Ticket
            {
                ReservationId = reservation.Id,
                QrCodeData = $"TICKET-{reservation.Id}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                GeneratedAt = DateTime.UtcNow
            };
            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();

            // 8. Envoi de la notification SMS (simulée)
            var travelerName = reservation.User?.Name ?? "Voyageur";
            var travelerPhone = reservation.User?.PhoneNumber ?? string.Empty;
            var departure = reservation.Trip?.DepartureCity ?? string.Empty;
            var arrival = reservation.Trip?.ArrivalCity ?? string.Empty;
            var tripDate = reservation.Trip?.Date.ToString("dd/MM/yyyy") ?? string.Empty;

            var smsMessage = $"Bonjour {travelerName}, votre réservation pour le trajet {departure} -> {arrival} du {tripDate} est confirmée ! Code ticket : {ticket.QrCodeData}.";
            await _notificationService.SendNotificationAsync(reservation.UserId, "SMS", travelerPhone, smsMessage);

            var dto = new PaymentDto
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, dto);
        }

        /// <summary>
        /// Récupère un paiement spécifique par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDto))]
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

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r!.Trip)
                        .ThenInclude(t => t!.Bus)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound(new { message = "Le paiement demandé n'existe pas." });
            }

            // Sécurité :
            // - Si Admin : accès total
            // - Si Voyageur : doit être le propriétaire de la réservation liée
            // - Si Compagnie : doit gérer le bus réalisant le trajet associé
            bool isAuthorized = User.IsInRole("Admin");

            if (!isAuthorized && User.IsInRole("Traveler"))
            {
                isAuthorized = payment.Reservation?.UserId == userId;
            }

            if (!isAuthorized && User.IsInRole("Company"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out var companyId))
                {
                    isAuthorized = payment.Reservation?.Trip?.Bus?.CompanyId == companyId;
                }
            }

            if (!isAuthorized)
            {
                return Forbid();
            }

            var dto = new PaymentDto
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Récupère le paiement associé à une réservation spécifique.
        /// </summary>
        [HttpGet("reservation/{reservationId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDto))]
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

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r!.Trip)
                        .ThenInclude(t => t!.Bus)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                return NotFound(new { message = "Aucun paiement trouvé pour cette réservation." });
            }

            // Sécurité identique
            bool isAuthorized = User.IsInRole("Admin");

            if (!isAuthorized && User.IsInRole("Traveler"))
            {
                isAuthorized = payment.Reservation?.UserId == userId;
            }

            if (!isAuthorized && User.IsInRole("Company"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out var companyId))
                {
                    isAuthorized = payment.Reservation?.Trip?.Bus?.CompanyId == companyId;
                }
            }

            if (!isAuthorized)
            {
                return Forbid();
            }

            var dto = new PaymentDto
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Met à jour le statut d'un paiement (Action d'administration ou callback de validation).
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                return BadRequest(new { message = "Le statut est obligatoire." });
            }

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound(new { message = "Le paiement demandé n'existe pas." });
            }

            payment.Status = newStatus;

            // Si le statut passe à Completed et que la réservation n'était pas confirmée, on la confirme et génère le ticket
            if (newStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                if (payment.Reservation != null && payment.Reservation.Status != ReservationStatus.Confirmed)
                {
                    payment.Reservation.Status = ReservationStatus.Confirmed;

                    // Vérifier si un ticket existe déjà
                    var ticketExists = await _context.Tickets.AnyAsync(t => t.ReservationId == payment.ReservationId);
                    if (!ticketExists)
                    {
                        var ticket = new Ticket
                        {
                            ReservationId = payment.ReservationId,
                            QrCodeData = $"TICKET-{payment.ReservationId}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            GeneratedAt = DateTime.UtcNow
                        };
                        _context.Tickets.Add(ticket);
                    }
                }
            }
            else if (newStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase) || newStatus.Equals("Refunded", StringComparison.OrdinalIgnoreCase))
            {
                // Si le paiement échoue ou est remboursé, on peut annuler la réservation si configuré
                if (payment.Reservation != null && payment.Reservation.Status == ReservationStatus.Confirmed)
                {
                    payment.Reservation.Status = ReservationStatus.Pending; // Repasse en attente ou annulé
                }
            }

            await _context.SaveChangesAsync();

            var dto = new PaymentDto
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };

            return Ok(dto);
        }
    }
}
