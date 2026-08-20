using ArtemisBankingPro.Application.DTOs.Admin;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Admin.Queries
{
    public class GetAdminHomeDataQuery : IRequest<AdminHomeIndicatorsDto> { }
}
