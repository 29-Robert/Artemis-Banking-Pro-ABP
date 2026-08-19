using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class CashAdvanceViewModel
    {
        [Required(ErrorMessage = "La cuenta de destino es requerida.")]
        [Display(Name = "Depositar en Cuenta")]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de tarjeta es requerido.")]
        [Display(Name = "Número de Tarjeta")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto del Avance (RD$)")]
        public decimal Amount { get; set; }
    }
}
