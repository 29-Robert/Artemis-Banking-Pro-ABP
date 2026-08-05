using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IBeneficiaryService
    {
        Task<BeneficiaryDto> AddBeneficiaryAsync(CreateBeneficiaryDto dto);

        Task RemoveBeneficiaryAsync(int id);

        Task TransferToBeneficiaryAsync(ExpressTransactionDto dto);
    }
}
