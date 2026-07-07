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
            var adminPhoneNormalized = dto.AdminPhoneNumber.Trim();
            var phoneExists = await _context.Users.AnyAsync(u => u.PhoneNumber == adminPhoneNormalized);
            if (phoneExists)
            {
                throw new InvalidOperationException("Ce numéro de téléphone est déjà associé à un compte.");
            }

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

            // Provisionne automatiquement le compte Company Admin rattaché à cette compagnie.
            var admin = new User
            {
                Name = dto.AdminName.Trim(),
                PhoneNumber = adminPhoneNormalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
                Role = UserRole.Company,
                CompanyId = company.Id
            };

            _context.Users.Add(admin);
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

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return false;
            }

            _context.Companies.Remove(company);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Impossible de supprimer cette compagnie car elle a encore un ou plusieurs comptes Company Admin (ou d'autres données) qui y sont rattachés.");
            }

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
