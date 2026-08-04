using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Domain.Entities;



namespace ArtemisBankingPro.Domain.Interfaces.Repositories
{
    public interface ICreditCardConsumptionRepository : IGenericRepository<CreditCardConsumption>
    {
       
        Task<IReadOnlyList<CreditCardConsumption>> GetConsumptionsByCardAsync(int creditCardId);
    }
}
