using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class CreateBeneficiaryViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El alias del beneficiario es requerido.")]
        [StringLength(100, ErrorMessage = "El alias no puede superar los 100 caracteres.")]
        public string Alias { get; set; } = string.Empty;
    }
}
