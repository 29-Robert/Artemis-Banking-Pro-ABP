using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Commerces
{
    public class PagedCommerceResponseDto
    {
        public int CurrentPage { get; set; } 
        public int TotalPages { get; set; } 
        public int TotalCount { get; set; } 
        public IEnumerable<CommerceDetailDto> Data { get; set; } = new List<CommerceDetailDto>();
    }
}
