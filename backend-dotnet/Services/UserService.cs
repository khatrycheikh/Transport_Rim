using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Data;
using TransportRim.Api.DTOs;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Implémentation du service gérant les utilisateurs de la plateforme.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly TransportRimDbContext _context;

        public UserService(TransportRimDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(string? search)
        {
            var query = _context.Users.Include(u => u.Company).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(u => u.Name.ToLower().Contains(term) || u.PhoneNumber.Contains(term));
            }

            var users = await query.ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<DeleteUserResult> DeleteAsync(int id, int requestingAdminId)
        {
            if (id == requestingAdminId)
            {
                return DeleteUserResult.CannotDeleteSelf;
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return DeleteUserResult.NotFound;
            }

            var hasReservations = await _context.Reservations.AnyAsync(r => r.UserId == id);
            if (hasReservations)
            {
                return DeleteUserResult.HasReservations;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return DeleteUserResult.Success;
        }

        private static UserDto MapToDto(Entities.User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
            };
        }
    }
}
