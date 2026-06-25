using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Data;
using TransportRim.Api.DTOs;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Implémentation du service gérant les compagnies.
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly TransportRimDbContext _context;

        public CompanyService(TransportRimDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies.ToListAsync();
            return companies.Select(c => MapToDto(c));
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            return company == null ? null : MapToDto(company);
        }

        public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto)
        {
            var company = new Company
            {
                Name = dto.Name.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email.ToLowerInvariant().Trim(),
                Address = dto.Address.Trim(),
                Status = "Pending", // Le statut initial est toujours en attente de validation par l'Admin
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return MapToDto(company);
        }

        public async Task<bool> UpdateCompanyStatusAsync(int id, string status)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return false;
            }

            company.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        private static CompanyDto MapToDto(Company company)
        {
            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Phone = company.Phone,
                Email = company.Email,
                Address = company.Address,
                Status = company.Status,
                CreatedAt = company.CreatedAt
            };
        }
    }
}
