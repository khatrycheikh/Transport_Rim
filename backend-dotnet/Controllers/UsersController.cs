using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportRim.Api.Services;

namespace TransportRim.Api.Controllers
{
    /// <summary>
    /// Contrôleur gérant la liste et la suppression des utilisateurs de la plateforme. Réservé à l'Administrateur.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Récupère la liste des utilisateurs, filtrée optionnellement par nom ou téléphone.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var result = await _userService.GetAllAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// Supprime un utilisateur (impossible pour soi-même ou un utilisateur ayant des réservations).
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminId))
            {
                return Unauthorized();
            }

            var result = await _userService.DeleteAsync(id, adminId);

            return result switch
            {
                DeleteUserResult.Success => NoContent(),
                DeleteUserResult.NotFound => NotFound(new { message = "L'utilisateur demandé n'existe pas." }),
                DeleteUserResult.CannotDeleteSelf => Conflict(new { message = "Vous ne pouvez pas supprimer votre propre compte." }),
                DeleteUserResult.HasReservations => Conflict(new { message = "Impossible de supprimer un utilisateur ayant des réservations existantes." }),
                _ => Conflict(),
            };
        }
    }
}
