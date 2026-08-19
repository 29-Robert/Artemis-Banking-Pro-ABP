using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class SelectClientViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente para continuar.")]
        public int SelectedClientId { get; set; }

        public string? Cedula { get; set; } 
    }
}