using ArtemisBankingPro.Application.DTOs.Commerces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ICommerceService
    {
        Task<CommerceListItemDto> CreateCommerceAsync(CreateCommerceDto dto);
        Task UpdateCommerceAsync(int id, UpdateCommerceDto dto);
        Task ChangeStatusAsync(int id, bool isActive);
        Task<PagedCommerceResponseDto> GetPagedCommercesAsync(int page, int limit);
        Task<CommerceDetailDto?> GetCommerceByIdAsync(int id);
    }
}
