using System.Threading.Tasks;
using TransportRim.Api.DTOs.Auth;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Service gérant la logique métier d'inscription et de connexion.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Enregistre un nouvel utilisateur après validation du numéro de téléphone unique.
        /// </summary>
        /// <param name="request">Les informations d'inscription.</param>
        /// <returns>Les informations de l'utilisateur authentifié avec son token, ou null si le numéro de téléphone existe déjà.</returns>
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// Connecte un utilisateur existant après vérification de ses identifiants.
        /// </summary>
        /// <param name="request">Les identifiants de connexion.</param>
        /// <returns>Les informations de l'utilisateur authentifié avec son token, ou null si la connexion échoue.</returns>
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    }
}
