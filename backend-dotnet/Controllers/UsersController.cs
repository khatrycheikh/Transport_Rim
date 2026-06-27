using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportRim.Api.DTOs;
using TransportRim.Api.Entities;
using TransportRim.Api.Services;

namespace TransportRim.Api.Controllers
{
    /// <summary>
    /// Contrôleur gérant les utilisateurs du système. Réservé aux administrateurs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Liste tous les utilisateurs de la plateforme, avec filtre optionnel par rôle.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] UserRole? role)
        {
            var result = await _userService.GetAllUsersAsync(role);
            return Ok(result);
        }

        /// <summary>
        /// Récupère un utilisateur spécifique par son identifiant.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "L'utilisateur demandé n'existe pas." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouvel utilisateur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto request)
        {
            try
            {
                var result = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour les informations d'un utilisateur existant.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto request)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "L'utilisateur demandé n'existe pas." });
                }

                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un utilisateur de la plateforme.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "L'utilisateur demandé n'existe pas." });
                }

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
