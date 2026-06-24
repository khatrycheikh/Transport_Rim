using TransportRim.Api.Entities;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Service pour la génération des jetons JWT.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Génère un token JWT signé pour l'utilisateur spécifié.
        /// </summary>
        /// <param name="user">L'utilisateur concerné.</param>
        /// <returns>Le token JWT sous forme de chaîne.</returns>
        string GenerateToken(User user);
    }
}
