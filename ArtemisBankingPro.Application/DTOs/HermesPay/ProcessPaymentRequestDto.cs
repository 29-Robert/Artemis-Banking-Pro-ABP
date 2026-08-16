namespace ArtemisBankingPro.Application.DTOs.HermesPay
{
    public class ProcessPaymentRequestDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public string ExpirationMonth { get; set; } = string.Empty;
        public string ExpirationYear { get; set; } = string.Empty;
        public string Cvc { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
