using System;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO représentant une notification envoyée.
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
