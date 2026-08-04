using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task ExpressTransactionAsync(ExpressTransactionDto dto);

        Task TransferBetweenOwnAccountsAsync(TransferOwnAccountDto dto);
    }
}
