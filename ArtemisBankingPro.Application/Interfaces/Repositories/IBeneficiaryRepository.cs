using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Repositories
{
    public interface IBeneficiaryRepository : IGenericRepository<Beneficiary>
    {
        Task<IEnumerable<Beneficiary>> GetByClientAsync(int clientId);
    }
}
