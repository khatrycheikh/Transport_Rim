using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportRim.Api.DTOs.Auth;
using TransportRim.Api.Services;

namespace TransportRim.Api.Controllers
{
    /// <summary>
    /// Contrôleur gérant les opérations d'authentification (Inscription et Connexion).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Permet à un utilisateur de créer un compte (Admin, Company, Traveler).
        /// </summary>
        /// <param name="request">Les informations d'inscription.</param>
        /// <returns>Les informations de profil et le jeton d'authentification JWT.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                if (result == null)
                {
                    return Conflict(new { message = "Le numéro de téléphone saisi est déjà associé à un compte." });
                }

                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Permet à un utilisateur de s'authentifier.
        /// </summary>
        /// <param name="request">Les identifiants de l'utilisateur.</param>
        /// <returns>Les informations de profil et le jeton d'authentification JWT.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized(new { message = "Le numéro de téléphone ou le mot de passe est incorrect." });
            }

            return Ok(result);
        }
    }
}
