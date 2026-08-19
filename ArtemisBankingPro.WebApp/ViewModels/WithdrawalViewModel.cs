using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class WithdrawalViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido.")]
        [Display(Name = "Cuenta origen")]
        public string SourceAccountNumber { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a retirar debe ser mayor que cero.")]
        [Display(Name = "Monto a retirar")]
        public decimal Amount { get; set; }
    }
}
