using System.ComponentModel.DataAnnotations;


namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class ThirdTransferViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido.")]
        [Display(Name = "Cuenta origen")]
        public string SourceAccountNumber { get; set; }

        [Required(ErrorMessage = "El número de cuenta destino es requerido.")]
        [Display(Name = "Cuenta destino")]
        public string DestinationAccountNumber { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a transferir debe ser mayor que cero.")]
        [Display(Name = "Monto a transferir")]
        public decimal Amount { get; set; }
    }
}
