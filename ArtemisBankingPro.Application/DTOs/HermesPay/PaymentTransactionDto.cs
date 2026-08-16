using System;

namespace ArtemisBankingPro.Application.DTOs.HermesPay
{
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }
}
