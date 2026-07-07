using System.Collections.Generic;
using System.Threading.Tasks;
using TransportRim.Api.DTOs;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Service gérant les opérations métiers relatives aux bus d'une compagnie.
    /// </summary>
    public interface IBusService
    {
        /// <summary>
        /// Récupère la liste de tous les bus d'une compagnie spécifique.
        /// </summary>
        Task<IEnumerable<BusDto>> GetBusesByCompanyIdAsync(int companyId);

        /// <summary>
        /// Récupère un bus spécifique par son ID et son appartenance à la compagnie.
        /// </summary>
        Task<BusDto?> GetBusByIdAsync(int id, int companyId);

        /// <summary>
        /// Crée un nouveau bus associé à la compagnie spécifiée.
        /// </summary>
        Task<BusDto> CreateBusAsync(CreateBusDto dto, int companyId);

        /// <summary>
        /// Met à jour les informations d'un bus sous condition de propriété.
        /// </summary>
        Task<BusDto?> UpdateBusAsync(int id, UpdateBusDto dto, int companyId);

        /// <summary>
        /// Supprime un bus sous condition de propriété.
        /// </summary>
        Task<bool> DeleteBusAsync(int id, int companyId);
    }
}
