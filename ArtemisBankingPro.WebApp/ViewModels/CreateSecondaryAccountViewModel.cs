using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class CreateSecondaryAccountViewModel
    {
        [Required(ErrorMessage = "La cédula del cliente es requerida.")]
        public string ClientCedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El balance inicial es requerido.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo.")]
        public decimal InitialBalance { get; set; }
    }
}
