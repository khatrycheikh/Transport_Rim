using System.Collections.Generic;
using System.Threading.Tasks;
using TransportRim.Api.DTOs;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Service gérant les opérations métiers pour les compagnies de transport.
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// Récupère la liste de toutes les compagnies enregistrées (Admin uniquement).
        /// </summary>
        Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync();

        /// <summary>
        /// Récupère les détails d'une compagnie par son ID.
        /// </summary>
        Task<CompanyDto?> GetCompanyByIdAsync(int id);

        /// <summary>
        /// Enregistre une nouvelle compagnie en attente d'approbation.
        /// </summary>
        Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto);

        /// <summary>
        /// Met à jour le statut d'une compagnie (Admin uniquement).
        /// </summary>
        Task<bool> UpdateCompanyStatusAsync(int id, string status);

        /// <summary>
        /// Supprime une compagnie (Admin uniquement).
        /// </summary>
        Task<bool> DeleteCompanyAsync(int id);
    }
}
