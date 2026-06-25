using System;

namespace TransportRim.Api.Entities
{
    /// <summary>
    /// Représente une notification (SMS ou Email) envoyée à un utilisateur.
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }

        public string Type { get; set; } = "SMS"; // SMS, Email
        public string Recipient { get; set; } = string.Empty; // Numéro de téléphone ou email
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
