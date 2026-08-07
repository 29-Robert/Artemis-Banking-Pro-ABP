using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class SavingsAccountService : ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRepository _loanRepository;

        public SavingsAccountService(
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            ILoanRepository loanRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _loanRepository = loanRepository;
        }

        public async Task<SavingsAccountDetailDto> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto)
        {
            if (dto.InitialBalance < 0)
                throw new Exception("El balance inicial no puede ser negativo.");

            var user = await _userRepository.GetByCedulaAsync(dto.ClientCedula);

            if (user == null)
                throw new Exception("No existe un cliente registrado con esta cédula.");

            if (!user.IsActive)
                throw new Exception("No se puede asignar una cuenta a un cliente inactivo.");

            int userId = user.Id;

            var principalAccount = await _accountRepository.GetPrincipalByClientAsync(userId);

            if (principalAccount == null || principalAccount.Status != AccountStatus.Activa)
                throw new Exception("El cliente debe tener una cuenta de ahorro principal activa antes de asignarle una cuenta secundaria.");

            string newAccountNumber;
            bool isUnique = false;
            var random = new Random();

            do
            {
                newAccountNumber = random.Next(100000000, 999999999).ToString();
                var existingAccount = await _accountRepository.GetByAccountNumberAsync(newAccountNumber);
                var existingLoan = await _loanRepository.GetByLoanNumberAsync(newAccountNumber);
                if (existingAccount == null && existingLoan == null)
                {
                    isUnique = true;
                }
            } while (!isUnique);

            var newAccount = new SavingsAccount
            {
                UserId = userId,
                AccountNumber = newAccountNumber,
                Balance = dto.InitialBalance,
                Type = AccountType.Secundaria,
                IsPrincipal = false,
                Status = AccountStatus.Activa,
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.AddAsync(newAccount);

            if (dto.InitialBalance > 0)
            {
                var initialTransaction = new Transaction
                {
                    AccountNumber = newAccount.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = dto.InitialBalance,
                    Status = TransactionStatus.Aprobada,
                    Description = "Asignación de balance inicial en cuenta secundaria",
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepository.AddAsync(initialTransaction);
            }

            return new SavingsAccountDetailDto
            {
                AccountNumber = newAccount.AccountNumber,
                Balance = newAccount.Balance,
                Status = newAccount.Status
            };
        }

        public async Task CancelSecondaryAccountAsync(string accountNumber)
        {
            var accountToCancel = await _accountRepository.GetByAccountNumberAsync(accountNumber);

            if (accountToCancel == null)
            {
                throw new Exception("La cuenta seleccionada no existe.");
            }

            if (accountToCancel.Status == AccountStatus.Cancelada)
            {
                throw new Exception("La cuenta seleccionada ya se encuentra cancelada.");
            }

            if (accountToCancel.IsPrincipal || accountToCancel.Type == AccountType.Principal)
            {
                throw new Exception("Las cuentas principales no pueden ser canceladas.");
            }

            var principalAccount = await _accountRepository.GetPrincipalByClientAsync(accountToCancel.UserId);

            if (principalAccount == null || principalAccount.Status != AccountStatus.Activa)
            {
                throw new Exception("No es posible cancelar la cuenta porque el cliente no tiene una cuenta principal activa para recibir los fondos.");
            }

            if (accountToCancel.Balance > 0 )
            {
                decimal transferAmount = accountToCancel.Balance;
                
                var debitTransaction = new Transaction
                {
                    AccountNumber = accountToCancel.AccountNumber,
                    Type = TransactionType.Debito,
                    Amount = transferAmount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia automática por cancelación hacia cuenta principal {principalAccount.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(debitTransaction);

                var creditTransaction = new Transaction
                {
                    AccountNumber = principalAccount.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = transferAmount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia recibida por cancelación de cuenta secundaria {accountToCancel.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(creditTransaction);

                principalAccount.Balance += transferAmount;
                accountToCancel.Balance = 0;

                await _accountRepository.UpdateAsync(principalAccount);
            }

            accountToCancel.Status = AccountStatus.Cancelada;

            await _accountRepository.UpdateAsync(accountToCancel);
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(string accountNumber, int page, int pageSize)
        {
            var transactions = await _transactionRepository.GetPagedByAccountAsync(accountNumber, page, pageSize);

            var history = transactions.Select(t => new TransactionDto
            {
                Date = t.CreatedAt,
                Amount = t.Amount,

                Type = t.Type,
                Status = t.Status,

                Beneficiary = t.Type == TransactionType.Debito ? t.RelatedEntity : accountNumber,
                Origin = t.Type == TransactionType.Credito ? t.RelatedEntity : accountNumber,

                Description = t.Description
            }).ToList();

            return history;
        }
    }
}