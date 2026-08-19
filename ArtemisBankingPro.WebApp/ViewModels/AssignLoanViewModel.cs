using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class AssignLoanViewModel
    {
        [Required(ErrorMessage = "El ID del cliente es requerido.")]
        [Display(Name = "ID del Cliente")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto de capital es requerido.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto de Capital (RD$)")]
        public decimal CapitalAmount { get; set; }

        [Required(ErrorMessage = "El plazo en meses es requerido.")]
        [Range(1, 360, ErrorMessage = "El plazo debe ser entre 1 y 360 meses.")]
        [Display(Name = "Plazo (meses)")]
        public int TermInMonths { get; set; }

        [Required(ErrorMessage = "La tasa de interés anual es requerida.")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01% y 100%.")]
        [Display(Name = "Tasa de Interés Anual (%)")]
        public decimal AnnualInterestRate { get; set; }

        /// <summary>
        /// Se activa cuando el admin confirma que desea asignar el préstamo
        /// a pesar de que el cliente es de alto riesgo.
        /// </summary>
        [Display(Name = "Confirmo que el cliente es de alto riesgo")]
        public bool ConfirmHighRisk { get; set; } = false;
    }
}
