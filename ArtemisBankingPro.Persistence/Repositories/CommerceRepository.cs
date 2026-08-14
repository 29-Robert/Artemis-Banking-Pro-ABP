using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class CommerceRepository : ICommerceRepository
    {
        public Task<Commerce> AddAsync(Commerce entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Commerce entity)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Commerce>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Commerce> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Commerce> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Commerce> GetByIdWithUserAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Commerce> GetByRncAsync(string rnc)
        {
            throw new NotImplementedException();
        }

        public Task<(IReadOnlyList<Commerce> Data, int TotalRecords)> GetPagedAsync(int page, int pageSize, string filter = null)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Commerce entity)
        {
            throw new NotImplementedException();
        }
    }
}
