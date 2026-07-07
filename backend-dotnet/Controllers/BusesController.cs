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
    /// Contrôleur gérant la flotte de bus. Réservé aux comptes "Company" (gestion de leur propre flotte).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Company")]
    public class BusesController : ControllerBase
    {
        private readonly IBusService _busService;

        public BusesController(IBusService busService)
        {
            _busService = busService;
        }

        /// <summary>
        /// Liste les bus de la compagnie courante.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BusDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var companyId = GetUserCompanyId();
            if (companyId == null)
            {
                return Forbid();
            }

            var result = await _busService.GetBusesByCompanyIdAsync(companyId.Value);
            return Ok(result);
        }

        /// <summary>
        /// Récupère un bus spécifique par son ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var companyId = GetUserCompanyId();
            var result = companyId == null ? null : await _busService.GetBusByIdAsync(id, companyId.Value);

            if (result == null)
            {
                return NotFound(new { message = "Le bus demandé n'existe pas ou n'appartient pas à votre compagnie." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Ajoute un nouveau bus à la compagnie courante.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BusDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateBusDto request)
        {
            var companyId = GetUserCompanyId();
            if (companyId == null)
            {
                return Forbid();
            }

            try
            {
                var result = await _busService.CreateBusAsync(request, companyId.Value);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour les informations d'un bus.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBusDto request)
        {
            var companyId = GetUserCompanyId();
            if (companyId == null)
            {
                return Forbid();
            }

            try
            {
                var result = await _busService.UpdateBusAsync(id, request, companyId.Value);
                if (result == null)
                {
                    return NotFound(new { message = "Le bus demandé n'existe pas ou n'appartient pas à votre compagnie." });
                }

                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un bus.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            try
            {
                var success = await _busService.DeleteBusAsync(id, companyId.Value);
                if (!success)
                {
                    return NotFound(new { message = "Le bus demandé n'existe pas ou n'appartient pas à votre compagnie." });
                }

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int? GetUserCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
