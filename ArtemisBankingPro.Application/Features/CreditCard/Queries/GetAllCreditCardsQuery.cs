using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Queries
{
    public class GetAllCreditCardsQuery : IRequest<PagedResult<CreditCardResponseDto>>
    {
        public string? Cedula { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
