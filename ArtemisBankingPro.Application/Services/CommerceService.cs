using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class CommerceService : ICommerceService
    {
        public Task ChangeStatusAsync(int id, bool isActive)
        {
            throw new NotImplementedException();
        }

        public Task<CommerceListItemDto> CreateCommerceAsync(CreateCommerceDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<CommerceDetailDto?> GetCommerceByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedCommerceResponseDto> GetPagedCommercesAsync(int page, int limit)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCommerceAsync(int id, UpdateCommerceDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
