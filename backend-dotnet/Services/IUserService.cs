using System.Collections.Generic;
using System.Threading.Tasks;
using TransportRim.Api.DTOs;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Contrat de service pour la gestion des utilisateurs.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Récupère la liste de tous les utilisateurs, avec un filtre par rôle optionnel.
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllUsersAsync(UserRole? roleFilter = null);

        /// <summary>
        /// Récupère un utilisateur par son identifiant unique.
        /// </summary>
        Task<UserDto?> GetUserByIdAsync(int id);

        /// <summary>
        /// Crée un nouvel utilisateur.
        /// </summary>
        Task<UserDto> CreateUserAsync(CreateUserDto dto);

        /// <summary>
        /// Met à jour les informations d'un utilisateur existant.
        /// </summary>
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);

        /// <summary>
        /// Supprime un utilisateur de la base de données.
        /// </summary>
        Task<bool> DeleteUserAsync(int id);
    }
}
