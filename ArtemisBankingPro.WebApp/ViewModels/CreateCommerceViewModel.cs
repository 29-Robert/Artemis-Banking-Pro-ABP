using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class CreateCommerceViewModel
    {
        [Required(ErrorMessage = "El nombre del comercio es requerido.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNC es requerido.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El RNC debe tener exactamente 9 dígitos numéricos.")]
        public string RNC { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es requerida.")]
        public string Address { get; set; } = string.Empty;
    }
}
