using System.Collections.Generic;
using System.Threading.Tasks;
using TransportRim.Api.DTOs;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Résultat possible d'une tentative de suppression d'utilisateur.
    /// </summary>
    public enum DeleteUserResult
    {
        Success,
        NotFound,
        CannotDeleteSelf,
        HasReservations
    }

    /// <summary>
    /// Service gérant les opérations métiers pour les utilisateurs de la plateforme (Admin uniquement).
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Récupère la liste des utilisateurs, filtrée optionnellement par nom ou téléphone.
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllAsync(string? search);

        /// <summary>
        /// Supprime un utilisateur, sous réserve qu'il ne s'agisse pas de l'administrateur courant
        /// et qu'il n'ait aucune réservation existante.
        /// </summary>
        Task<DeleteUserResult> DeleteAsync(int id, int requestingAdminId);
    }
}
