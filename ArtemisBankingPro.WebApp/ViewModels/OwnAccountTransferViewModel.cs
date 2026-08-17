using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class OwnAccountTransferViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es requerida.")]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cuenta de destino es requerida.")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto a transferir es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a transferir debe ser mayor que cero.")]
        public decimal Amount { get; set; }
    }
}
