using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class CreditCardPaymentViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido.")]
        [Display(Name = "Cuenta origen")]
        public string SourceAccountNumber { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es requerido.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe contener 16 dígitos.")]
        [Display(Name = "Número de tarjeta")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a pagar debe ser mayor que cero.")]
        [Display(Name = "Monto a pagar")]
        public decimal Amount { get; set; }
    }
}
