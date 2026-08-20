using ArtemisBankingPro.Application.DTOs;

namespace ArtemisBankingPro.WebApp.ViewModels
{
    public class SelectClientForCreditCardViewModel
    {
        public string SearchCedula { get; set; } = string.Empty;
        public decimal AverageDebt { get; set; }
        public IEnumerable<ClientDebtDto> Clients { get; set; } = new List<ClientDebtDto>();
        public int? SelectedClientId { get; set; }
    }
}
