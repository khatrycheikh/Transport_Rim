using System;
using System.Threading.Tasks;
using TransportRim.Api.Data;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Implémentation du service de notifications.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly TransportRimDbContext _context;

        public NotificationService(TransportRimDbContext context)
        {
            _context = context;
        }

        public async Task SendNotificationAsync(int userId, string type, string recipient, string message)
        {
            // 1. Simulation d'envoi (écriture dans la console du serveur)
            Console.WriteLine($"[NOTIFICATION - {type.ToUpper()}] Destinataire: {recipient} | Message: \"{message}\"");

            // 2. Enregistrement en base de données pour l'historique de l'utilisateur
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Recipient = recipient,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
