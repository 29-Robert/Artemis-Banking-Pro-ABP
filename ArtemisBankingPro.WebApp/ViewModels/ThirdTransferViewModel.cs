using System.ComponentModel.DataAnnotations;




namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class ThirdTransferViewModel
    {
        [Required]
        [Display(Name = "Cuenta origen")]
        public string SourceAccountNumber { get; set; }

        [Required]
        [Display(Name = "Cuenta destino")]
        public string TargetAccountNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Monto")]
        public decimal Amount { get; set; }
    }
}
