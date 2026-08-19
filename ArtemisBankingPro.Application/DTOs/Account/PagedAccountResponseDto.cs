using ArtemisBankingPro.Domain.Entities;
using System.Collections.Generic;

namespace ArtemisBankingPro.Application.DTOs.Account
{
    public class PagedAccountResponseDto
    {
        public IEnumerable<SavingsAccount> Data { get; set; } = new List<SavingsAccount>();
        public int TotalCount { get; set; }
    }
}
