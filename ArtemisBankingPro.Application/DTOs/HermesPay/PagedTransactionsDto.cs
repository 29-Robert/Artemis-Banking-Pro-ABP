using System.Collections.Generic;

namespace ArtemisBankingPro.Application.DTOs.HermesPay
{
    public class PagedTransactionsDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<PaymentTransactionDto> Data { get; set; } = new List<PaymentTransactionDto>();
    }
}
