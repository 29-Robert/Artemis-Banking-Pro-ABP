using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class LoanPaymentViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido.")]
        [Display(Name = "Cuenta origen")]
        public string SourceAccountNumber { get; set; }

        [Required(ErrorMessage = "El número de préstamo es requerido.")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de préstamo debe contener 9 dígitos.")]
        [Display(Name = "Número de préstamo")]
        public string LoanNumber { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a pagar debe ser mayor que cero.")]
        [Display(Name = "Monto a pagar")]
        public decimal Amount { get; set; }
    }
}
