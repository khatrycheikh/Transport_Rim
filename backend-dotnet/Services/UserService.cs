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
    /// Service d'implémentation de la gestion des utilisateurs.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly TransportRimDbContext _context;

        public UserService(TransportRimDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(UserRole? roleFilter = null)
        {
            var query = _context.Users.Include(u => u.Company).AsQueryable();

            if (roleFilter.HasValue)
            {
                query = query.Where(u => u.Role == roleFilter.Value);
            }

            var users = await query.ToListAsync();
            return users.Select(u => MapToDto(u));
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var phoneNormalized = dto.PhoneNumber.Trim();

            // Éviter les doublons de numéro de téléphone
            var exists = await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNormalized);
            if (exists)
            {
                throw new InvalidOperationException("Ce numéro de téléphone est déjà associé à un compte.");
            }

            int? assignedCompanyId = null;
            if (dto.Role == UserRole.Company)
            {
                if (!dto.CompanyId.HasValue || dto.CompanyId.Value <= 0)
                {
                    throw new InvalidOperationException("Un identifiant de compagnie valide (CompanyId) est obligatoire pour le rôle Company.");
                }

                var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value);
                if (!companyExists)
                {
                    throw new InvalidOperationException("La compagnie de transport spécifiée n'existe pas.");
                }

                assignedCompanyId = dto.CompanyId.Value;
            }

            var user = new User
            {
                Name = dto.Name,
                PhoneNumber = phoneNormalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                CompanyId = assignedCompanyId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Recharger pour inclure la compagnie
            var createdUser = await _context.Users
                .Include(u => u.Company)
                .FirstAsync(u => u.Id == user.Id);

            return MapToDto(createdUser);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }

            var phoneNormalized = dto.PhoneNumber.Trim();

            // Vérification de l'unicité du téléphone s'il a changé
            if (user.PhoneNumber != phoneNormalized)
            {
                var phoneExists = await _context.Users.AnyAsync(u => u.Id != id && u.PhoneNumber == phoneNormalized);
                if (phoneExists)
                {
                    throw new InvalidOperationException("Ce numéro de téléphone est déjà associé à un autre compte.");
                }
            }

            int? assignedCompanyId = null;
            if (dto.Role == UserRole.Company)
            {
                if (!dto.CompanyId.HasValue || dto.CompanyId.Value <= 0)
                {
                    throw new InvalidOperationException("Un identifiant de compagnie valide (CompanyId) est obligatoire pour le rôle Company.");
                }

                var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value);
                if (!companyExists)
                {
                    throw new InvalidOperationException("La compagnie de transport spécifiée n'existe pas.");
                }

                assignedCompanyId = dto.CompanyId.Value;
            }

            user.Name = dto.Name;
            user.PhoneNumber = phoneNormalized;
            user.Role = dto.Role;
            user.CompanyId = assignedCompanyId;

            // Mettre à jour le mot de passe s'il est fourni (réinitialisation par l'admin)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            // Recharger pour inclure la compagnie
            var updatedUser = await _context.Users
                .Include(u => u.Company)
                .FirstAsync(u => u.Id == user.Id);

            return MapToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return false;
            }

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name ?? string.Empty
            };
        }
    }
}
