using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class EditCreditLimitViewModel
    {
        public int CardId { get; set; }

        [Display(Name = "Número de tarjeta")]
        public string? MaskedCardNumber { get; set; } 

        [Required(ErrorMessage = "El límite es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de la tarjeta debe ser mayor que cero.")]
        [Display(Name = "Nuevo límite de crédito")]
        public decimal NewCreditLimit { get; set; }
    }
}