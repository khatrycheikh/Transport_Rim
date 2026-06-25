using System.Threading.Tasks;

namespace TransportRim.Api.Services
{
    /// <summary>
    /// Service gérant la simulation d'envoi et la journalisation des notifications.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Envoie et enregistre une notification pour un utilisateur.
        /// </summary>
        /// <param name="userId">L'identifiant de l'utilisateur destinataire.</param>
        /// <param name="type">Le type ("SMS" ou "Email").</param>
        /// <param name="recipient">L'adresse e-mail ou le numéro de téléphone.</param>
        /// <param name="message">Le texte du message.</param>
        Task SendNotificationAsync(int userId, string type, string recipient, string message);
    }
}
