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
    /// Contrôleur gérant les opérations CRUD et l'administration des compagnies de transport.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// Permet de soumettre une demande d'inscription pour une nouvelle compagnie.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompanyDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto request)
        {
            var result = await _companyService.CreateCompanyAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Récupère la liste de toutes les compagnies. Réservé à l'Administrateur.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _companyService.GetAllCompaniesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Récupère les détails d'une compagnie spécifique.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _companyService.GetCompanyByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "La compagnie demandée n'existe pas." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Modifie le statut d'une compagnie (ex: approuver ou suspendre). Réservé à l'Administrateur.
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCompanyStatusDto request)
        {
            var success = await _companyService.UpdateCompanyStatusAsync(id, request.Status);
            if (!success)
            {
                return NotFound(new { message = "La compagnie demandée n'existe pas." });
            }

            return NoContent();
        }
    }
}
