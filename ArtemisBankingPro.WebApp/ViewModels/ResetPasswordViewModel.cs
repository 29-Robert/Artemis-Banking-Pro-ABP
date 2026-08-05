using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar su contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}