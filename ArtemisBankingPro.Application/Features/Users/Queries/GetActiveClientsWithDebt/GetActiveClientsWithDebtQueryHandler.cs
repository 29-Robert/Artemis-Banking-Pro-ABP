using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetActiveClientsWithDebtQueryHandler(
        IGenericRepository<User> userRepository,
        ILoanRepository loanRepository,
        ICreditCardRepository creditCardRepository) : IRequestHandler<GetActiveClientsWithDebtQuery, IEnumerable<ClientDebtDto>>
    {
        public async Task<IEnumerable<ClientDebtDto>> Handle(GetActiveClientsWithDebtQuery request, CancellationToken cancellationToken)
        {
            var allUsers = await userRepository.GetAllAsync();
            var activeClients = allUsers.Where(u => u.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Cliente && u.IsActive).ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchCedula))
            {
                activeClients = activeClients.Where(u => u.Cedula.Contains(request.SearchCedula)).ToList();
            }

            var result = new List<ClientDebtDto>();

            foreach (var client in activeClients)
            {
                var loans = await loanRepository.GetLoansByClientAsync(client.Id);
                var activeLoansDebt = loans.Where(l => l.Status == "Activo" || l.Status == "Aprobado").Sum(l => l.CapitalAmount);

                var cards = await creditCardRepository.GetCardsByClientAsync(client.Id);
                var activeCardsDebt = cards.Where(c => c.Status == "Activa").Sum(c => c.CurrentDebt);

                decimal totalDebt = activeLoansDebt + activeCardsDebt;

                result.Add(new ClientDebtDto
                {
                    Id = client.Id,
                    Cedula = client.Cedula,
                    FullName = $"{client.FirstName} {client.LastName}",
                    Email = client.Email,
                    TotalDebt = totalDebt
                });
            }

            return result;
        }
    }
}
