using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class UpdateCommerceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del comercio es requerido.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNC es requerido.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El RNC debe tener exactamente 9 dígitos numéricos.")]
        public string RNC { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es requerida.")]
        public string Address { get; set; } = string.Empty;
    }
}
