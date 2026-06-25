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
    /// Contrôleur gérant les opérations de gestion de profil des utilisateurs.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly TransportRimDbContext _context;
        private readonly ITokenService _tokenService;

        public ProfilesController(TransportRimDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Met à jour le profil de l'utilisateur connecté (nom et numéro de téléphone).
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé." });
            }

            var newPhone = request.PhoneNumber.Trim();

            // Vérification de l'unicité du numéro de téléphone s'il a changé
            if (user.PhoneNumber != newPhone)
            {
                var phoneExists = await _context.Users.AnyAsync(u => u.Id != userId && u.PhoneNumber == newPhone);
                if (phoneExists)
                {
                    return Conflict(new { message = "Ce numéro de téléphone est déjà associé à un autre compte." });
                }
            }

            // Application des modifications
            user.Name = request.Name;
            user.PhoneNumber = newPhone;

            await _context.SaveChangesAsync();

            // Génération d'un nouveau token avec les claims mis à jour
            var newToken = _tokenService.GenerateToken(user);

            var dto = new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Token = newToken
            };

            return Ok(dto);
        }

        /// <summary>
        /// Permet de modifier le mot de passe de l'utilisateur connecté.
        /// </summary>
        [HttpPut("password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé." });
            }

            // Vérification du mot de passe actuel
            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                return BadRequest(new { message = "Le mot de passe actuel saisi est incorrect." });
            }

            // Hachage et mise à jour
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Votre mot de passe a été mis à jour avec succès." });
        }
    }
}
