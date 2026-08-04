using ArtemisBankingPro.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IHermesPayService
    {
        Task<PaymentResultDto> ProcessPaymentAsync(int commerceId, PaymentDto dto);

        Task<IEnumerable<TransactionDto>> GetTransactionsAsync(int commerceId, int page, int pageSize);
    }
}
