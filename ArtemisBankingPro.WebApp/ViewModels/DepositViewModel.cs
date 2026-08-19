using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.Models
{
    public class DepositViewModel
    {
        [Required(ErrorMessage = "El número de cuenta destino es requerido.")]
        [Display(Name = "Cuenta destino")]
        public string DestinationAccountNumber { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a depositar debe ser mayor que cero.")]
        [Display(Name = "Monto a depositar")]
        public decimal Amount { get; set; }
    }
}
