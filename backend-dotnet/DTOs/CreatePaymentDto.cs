using System.ComponentModel.DataAnnotations;
using TransportRim.Api.Entities;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO pour la création d'un paiement de réservation.
    /// </summary>
    public class CreatePaymentDto
    {
        [Required(ErrorMessage = "L'identifiant de la réservation est obligatoire.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "La méthode de paiement est obligatoire.")]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Méthode de paiement invalide (Bankily, Masrivi, Cash).")]
        public PaymentMethod Method { get; set; }

        [Required(ErrorMessage = "Le numéro de transaction est obligatoire.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Le numéro de transaction doit comporter entre 3 et 100 caractères.")]
        public string TransactionId { get; set; } = string.Empty;
    }
}
