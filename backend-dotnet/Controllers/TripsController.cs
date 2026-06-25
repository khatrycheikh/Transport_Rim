using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportRim.Api.DTOs;
using TransportRim.Api.Services;

namespace TransportRim.Api.Controllers
{
    /// <summary>
    /// Contrôleur gérant la recherche, planification et suppression des trajets.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        /// <summary>
        /// Permet de rechercher des trajets selon des critères de départ, arrivée et date. (Public)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TripDto>))]
        public async Task<IActionResult> Search(
            [FromQuery] string? departureCity, 
            [FromQuery] string? arrivalCity, 
            [FromQuery] DateTime? date)
        {
            var result = await _tripService.SearchTripsAsync(departureCity, arrivalCity, date);
            return Ok(result);
        }

        /// <summary>
        /// Récupère les informations détaillées d'un trajet par son ID. (Public)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TripDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tripService.GetTripByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Le trajet demandé n'existe pas." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Permet à une compagnie de planifier un nouveau trajet. (Compagnie uniquement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TripDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateTripDto request)
        {
            var companyId = GetUserCompanyId();
            if (companyId == null)
            {
                return Forbid();
            }

            try
            {
                var result = await _tripService.CreateTripAsync(request, companyId.Value);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère la liste de tous les trajets planifiés par la compagnie de l'utilisateur connecté. (Compagnie uniquement)
        /// </summary>
        [HttpGet("company")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TripDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCompanyTrips()
        {
            var companyId = GetUserCompanyId();
            if (companyId == null)
            {
                return Forbid();
            }

            var result = await _tripService.GetTripsByCompanyIdAsync(companyId.Value);
            return Ok(result);
        }

        /// <summary>
        /// Supprime un trajet. (Compagnie uniquement)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetUserCompanyId();
            if (companyId == null)
            {
                return Forbid();
            }

            var success = await _tripService.DeleteTripAsync(id, companyId.Value);
            if (!success)
            {
                return NotFound(new { message = "Le trajet demandé n'existe pas ou n'appartient pas à votre compagnie." });
            }

            return NoContent();
        }

        private int? GetUserCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
