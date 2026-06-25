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
    /// Contrôleur permettant aux utilisateurs de consulter leur historique de notifications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly TransportRimDbContext _context;

        public NotificationsController(TransportRimDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Liste l'historique des notifications reçues par l'utilisateur connecté.
        /// Un administrateur peut voir toutes les notifications.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var query = _context.Notifications.AsQueryable();

            // Si ce n'est pas un admin, on filtre par son UserId
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(n => n.UserId == userId);
            }

            var notifications = await query
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            var dtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Recipient = n.Recipient,
                Message = n.Message,
                SentAt = n.SentAt
            }).ToArray();

            return Ok(dtos);
        }
    }
}
