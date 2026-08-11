using System.ComponentModel.DataAnnotations;



namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class LoanPaymentViewModel
    {
        [Required]
        [Display(Name = "Cuenta origen")]
        public string SourceAccountNumber { get; set; }

        [Required]
        [Display(Name = "Número de préstamo")]
        public string LoanNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Monto")]
        public decimal Amount { get; set; }

    }
}
