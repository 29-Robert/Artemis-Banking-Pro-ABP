using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IBeneficiaryService
    {
        Task<BeneficiaryResponseDto> AddBeneficiaryAsync(AddBeneficiaryDto dto);

        Task RemoveBeneficiaryAsync(int id);

        Task TransferToBeneficiaryAsync(TransferDto dto);
    }
}
