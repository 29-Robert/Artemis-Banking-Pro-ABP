using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.DTOs.Transactions;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IBeneficiaryService
    {
        Task<BeneficiaryDto> AddBeneficiaryAsync(int clientId, CreateBeneficiaryDto dto);
        Task RemoveBeneficiaryAsync(int id);
        Task TransferToBeneficiaryAsync(ExpressTransactionDto dto);
    }
}
