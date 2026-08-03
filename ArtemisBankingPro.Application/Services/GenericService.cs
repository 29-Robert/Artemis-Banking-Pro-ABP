using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Interfaces.Repositories;

namespace ArtemisBankingPro.Application.Services
{
    public class GenericService<T>(IGenericRepository<T> repository) : IGenericService<T> where T : class
    {
        protected readonly IGenericRepository<T> _repository = repository;

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}