using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Data;
using TransportRim.Api.DTOs.Auth;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Implémentation du service d'authentification.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly TransportRimDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(TransportRimDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            // Éviter les doublons de numéro de téléphone (protection contre l'injection SQL assurée par EF Core paramétré)
            var exists = await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (exists)
            {
                return null;
            }

            // Hachage sécurisé du mot de passe avec BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Validation et affectation du CompanyId selon le rôle
            int? assignedCompanyId = null;

            if (request.Role == UserRole.Company)
            {
                if (!request.CompanyId.HasValue || request.CompanyId.Value <= 0)
                {
                    throw new InvalidOperationException("Un identifiant de compagnie valide (CompanyId) est obligatoire pour le rôle Company.");
                }

                var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId.Value);
                if (!companyExists)
                {
                    throw new InvalidOperationException("La compagnie de transport spécifiée n'existe pas.");
                }

                assignedCompanyId = request.CompanyId.Value;
            }

            var user = new User
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = passwordHash,
                Role = request.Role,
                CompanyId = assignedCompanyId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Génération du token JWT après inscription réussie
            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Token = token
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var phoneTrimmed = request.PhoneNumber.Trim();
            
            // Recherche de l'utilisateur par téléphone
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneTrimmed);
            if (user == null)
            {
                return null;
            }

            // Vérification de la correspondance du mot de passe
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null;
            }

            // Génération du token
            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Token = token
            };
        }
    }
}
