using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;
    }
}