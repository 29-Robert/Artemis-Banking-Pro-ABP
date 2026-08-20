using ArtemisBankingPro.Application.DTOs.Admin;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Admin.Queries
{
    public class GetAdminHomeIndicatorsQueryHandler : IRequestHandler<GetAdminHomeDataQuery, AdminHomeIndicatorsDto>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _creditCardRepository;

        public GetAdminHomeIndicatorsQueryHandler(
            ITransactionRepository transactionRepository,
            IGenericRepository<User> userRepository,
            ISavingsAccountRepository accountRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository creditCardRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _loanRepository = loanRepository;
            _creditCardRepository = creditCardRepository;
        }

        public async Task<AdminHomeIndicatorsDto> Handle(GetAdminHomeDataQuery request, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;

            // Transacciones y pagos
            var transactions = await _transactionRepository.GetAllApprovedAsync();

            var totalTransactionsHistorical = transactions.Count;
            var totalTransactionsToday = transactions.Count(t => t.CreatedAt.Date == today);

            bool IsPayment(Transaction t) =>
                t.Type == TransactionType.Debito &&
                (t.Description.StartsWith("PAGO A TARJETA", StringComparison.OrdinalIgnoreCase) ||
                 t.Description.StartsWith("PAGO A PRÉSTAMO", StringComparison.OrdinalIgnoreCase));

            var totalPaymentsHistorical = transactions.Count(IsPayment);
            var totalPaymentsToday = transactions.Count(t => IsPayment(t) && t.CreatedAt.Date == today);

            // Clientes
            var allUsers = await _userRepository.GetAllAsync();
            var clients = allUsers.Where(u => u.RoleId == (int)Roles.Cliente).ToList();

            var activeClients = clients.Count(c => c.IsActive);
            var inactiveClients = clients.Count(c => !c.IsActive);

            // Productos financieros activos
            var allAccounts = await _accountRepository.GetAllAsync();
            var activeAccounts = allAccounts.Count(a => a.Status == AccountStatus.Activa);

            var allLoans = await _loanRepository.GetAllAsync();
            var activeLoans = allLoans.Count(l => l.Status == "Activo");

            var allCards = await _creditCardRepository.GetAllAsync();
            var activeCards = allCards.Count(c => c.Status == "Activa");

            var activeFinancialProducts = activeAccounts + activeLoans + activeCards;

            // Deuda promedio por cliente
            var totalLoanDebt = await _loanRepository.GetTotalActiveDebtSystemWideAsync();
            var totalCardDebt = await _creditCardRepository.GetTotalActiveDebtSystemWideAsync();
            var averageDebtPerClient = activeClients == 0 ? 0m : (totalLoanDebt + totalCardDebt) / activeClients;

            return new AdminHomeIndicatorsDto
            {
                TotalTransactionsHistorical = totalTransactionsHistorical,
                TotalTransactionsToday = totalTransactionsToday,
                TotalPaymentsHistorical = totalPaymentsHistorical,
                TotalPaymentsToday = totalPaymentsToday,
                ActiveClients = activeClients,
                InactiveClients = inactiveClients,
                ActiveFinancialProducts = activeFinancialProducts,
                AverageDebtPerClient = averageDebtPerClient
            };
        }
    }
}