using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class AssignCreditCardViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente para continuar.")]
        [Display(Name = "Cliente")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "El límite de crédito es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor que cero.")]
        [Display(Name = "Límite de crédito")]
        public decimal CreditLimit { get; set; }
    }
}
