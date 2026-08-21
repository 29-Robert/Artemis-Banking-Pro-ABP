using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class ResendActivationViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el nombre de usuario o correo electrÃ³nico.")]
        public string EmailOrUsername { get; set; } = string.Empty;
    }
}
