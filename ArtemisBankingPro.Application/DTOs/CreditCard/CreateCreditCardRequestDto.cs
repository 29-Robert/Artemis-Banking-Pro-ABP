namespace ArtemisBankingPro.Application.DTOs.CreditCard
{
    public class CreateCreditCardRequestDto
    {
        public string ClientId { get; set; }
        public decimal CreditLimit { get; set; }
    }
}