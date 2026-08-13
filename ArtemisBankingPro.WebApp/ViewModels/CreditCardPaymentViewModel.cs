using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class CreditCardPaymentViewModel
    {
        [Required]
        [Display(Name = "Cuenta de origen")]
        public string SourceAccountNumber { get; set; }

        [Required]
        [Display(Name = "Número de tarjeta")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Monto")]
        public decimal Amount { get; set; }
    }
}
