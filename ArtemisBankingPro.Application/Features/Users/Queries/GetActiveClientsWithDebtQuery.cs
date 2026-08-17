using ArtemisBankingPro.Application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetActiveClientsWithDebtQuery : IRequest<IEnumerable<ClientDebtDto>>
    {
        public string SearchCedula { get; set; } = string.Empty;
    }
}
