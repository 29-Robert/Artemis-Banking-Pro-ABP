namespace ArtemisBankingPro.Application.DTOs.CreditCard
{
    public class CreditCardCreatedResponseDto : CreditCardResponseDto
    {
        public string CardNumber { get; set; }
        public string Cvc { get; set; }
    }
}