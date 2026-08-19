using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class EditLoanRateViewModel
    {
        public int LoanId { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public decimal CurrentRate { get; set; }

        [Required(ErrorMessage = "La nueva tasa es requerida.")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01% y 100%.")]
        [Display(Name = "Nueva Tasa de Interés Anual (%)")]
        public decimal NewRate { get; set; }
    }
}
