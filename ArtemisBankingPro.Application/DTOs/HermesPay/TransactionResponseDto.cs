using System;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Application.DTOs.HermesPay
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public TransactionStatus Status { get; set; } 
        public string AuthorizationCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
