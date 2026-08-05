using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IHermesPayService
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(int commerceId, ProcessPaymentDto dto);

        Task<IEnumerable<TransactionDto>> GetTransactionsAsync(int commerceId, int page, int pageSize);
    }
}
