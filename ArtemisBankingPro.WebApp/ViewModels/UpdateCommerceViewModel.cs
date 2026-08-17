using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class UpdateCommerceViewModel
    {
        [Required(ErrorMessage = "El nombre del comercio es requerido.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo del comercio es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono del comercio es requerido.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de cuenta del comercio es requerido.")]
        public string AccountNumber { get; set; } = string.Empty;
    }
}
