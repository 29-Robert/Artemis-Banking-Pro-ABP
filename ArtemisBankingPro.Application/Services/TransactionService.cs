using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Transactions;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task ExpressTransactionAsync(ExpressTransactionDto dto)
        {
            if (dto.Amount <= 0) throw new Exception("El monto a transferir debe ser mayor que cero.");
            if (dto.SourceAccountNumber == dto.DestinationAccountNumber) throw new Exception("La cuenta de origen y destino no pueden ser la misma.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var srcAcc = await _accountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
                if (srcAcc == null) throw new Exception("La cuenta de origen no existe.");
                if (srcAcc.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta de origen se encuentra cancelada.");
                if (srcAcc.Balance < dto.Amount) throw new Exception("Fondos insuficientes en la cuenta de origen.");

                var tgtAcc = await _accountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);
                if (tgtAcc == null) throw new Exception("La cuenta de destino no existe.");
                if (tgtAcc.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta de destino se encuentra cancelada.");

                srcAcc.Balance -= dto.Amount;
                tgtAcc.Balance += dto.Amount;

                await _accountRepository.UpdateAsync(srcAcc);
                await _accountRepository.UpdateAsync(tgtAcc);

                var debitTx = new Transaction
                {
                    AccountNumber = dto.SourceAccountNumber,
                    Type = TransactionType.Debito,
                    Amount = dto.Amount,
                    Status = TransactionStatus.Aprobada,
                    Description = "Transferencia Express",
                    RelatedEntity = dto.DestinationAccountNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(debitTx);

                var creditTx = new Transaction
                {
                    AccountNumber = dto.DestinationAccountNumber,
                    Type = TransactionType.Credito,
                    Amount = dto.Amount,
                    Status = TransactionStatus.Aprobada,
                    Description = "Transferencia Express",
                    RelatedEntity = dto.SourceAccountNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(creditTx);

                await _unitOfWork.CommitAsync();

                try
                {
                    string body = $"Hola,\nSe ha realizado una transferencia Express desde tu cuenta propia.\n" +
                                  $"Cuenta de ahorro: {dto.SourceAccountNumber}\n" +
                                  $"Cuenta de destino: {dto.DestinationAccountNumber}\n" +
                                  $"Monto transferido: RD$ {dto.Amount:N2}\n" +
                                  $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                    
                    string emailOrigen = srcAcc.User?.Email ?? "soporte@artemisbanking.com";
                    await _emailService.SendNotificationEmailAsync(emailOrigen, "Transferencia Express Realizada", body);
                }
                catch { }
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<AccountResponseDto> OwnAccountTransferAsync(OwnAccountTransferDto dto, string clientId)
        {
            int idCliente = int.Parse(clientId);

            if (dto.SourceAccountNumber == dto.DestinationAccountNumber)
            {
                await LogRejectedTransactionAsync(dto.SourceAccountNumber, dto.Amount, "La cuenta de origen y la cuenta de destino no pueden ser la misma.");
                return new AccountResponseDto { IsSuccess = false, Message = "La cuenta de origen y la cuenta de destino no pueden ser la misma." };
            }

            if (dto.Amount <= 0)
            {
                await LogRejectedTransactionAsync(dto.SourceAccountNumber, dto.Amount, "El monto a transferir debe ser mayor que cero.");
                return new AccountResponseDto { IsSuccess = false, Message = "El monto a transferir debe ser mayor que cero." };
            }

            var activeAccountsCount = await _accountRepository.CountActiveAccountsByClientIdAsync(idCliente);
            if (activeAccountsCount < 2)
            {
                return new AccountResponseDto { IsSuccess = false, Message = "Debe tener al menos dos cuentas de ahorro activas para realizar una transferencia entre cuentas." };
            }

            var sourceAccount = await _accountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
            var destAccount = await _accountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);

            if (sourceAccount == null || sourceAccount.UserId != idCliente || destAccount == null || destAccount.UserId != idCliente)
            {
                return new AccountResponseDto { IsSuccess = false, Message = "Las cuentas seleccionadas no son válidas o no le pertenecen." };
            }

            if (sourceAccount.Status == AccountStatus.Cancelada || destAccount.Status == AccountStatus.Cancelada)
            {
                return new AccountResponseDto { IsSuccess = false, Message = "Operación denegada. Una o ambas cuentas se encuentran canceladas." };
            }

            if (sourceAccount.Balance < dto.Amount)
            {
                await LogRejectedTransactionAsync(dto.SourceAccountNumber, dto.Amount, "No dispone del monto requerido en la cuenta seleccionada.");
                return new AccountResponseDto { IsSuccess = false, Message = "No dispone del monto requerido en la cuenta seleccionada." };
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                sourceAccount.Balance -= dto.Amount;
                destAccount.Balance += dto.Amount;

                await _accountRepository.UpdateAsync(sourceAccount);
                await _accountRepository.UpdateAsync(destAccount);

                var debitTransaction = new Transaction
                {
                    AccountNumber = sourceAccount.AccountNumber,
                    Type = TransactionType.Debito,
                    Amount = dto.Amount,
                    Status = TransactionStatus.Aprobada,
                    Description = "Transferencia Express",
                    RelatedEntity = destAccount.AccountNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(debitTransaction);

                var creditTransaction = new Transaction
                {
                    AccountNumber = destAccount.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = dto.Amount,
                    Status = TransactionStatus.Aprobada,
                    Description = "Transferencia Express",
                    RelatedEntity = sourceAccount.AccountNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(creditTransaction);

                await _unitOfWork.CommitAsync();

                try
                {
                    string maskSource = dto.SourceAccountNumber.Substring(dto.SourceAccountNumber.Length - 4);
                    string maskDest = dto.DestinationAccountNumber.Substring(dto.DestinationAccountNumber.Length - 4);

                    string body = $"Hola,\nSe ha realizado una transferencia entre sus cuentas de ahorro.\n" +
                                  $"Cuenta origen terminada en: {maskSource}\n" +
                                  $"Cuenta destino terminada en: {maskDest}\n" +
                                  $"Monto transferido: RD${dto.Amount}\n" +
                                  $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                                  $"Si usted no reconoce esta operación, comuníquese con la entidad bancaria.";

                    string emailDestino = sourceAccount.User?.Email ?? destAccount.User?.Email;

                    if (!string.IsNullOrEmpty(emailDestino))
                    {
                        await _emailService.SendNotificationEmailAsync(emailDestino, "Transferencia entre cuentas realizada", body);
                    }

                    return new AccountResponseDto { IsSuccess = true, Message = "Transferencia realizada con éxito." };
                }
                catch
                {
                    return new AccountResponseDto { IsSuccess = true, Message = "La transferencia fue realizada correctamente, pero no fue posible enviar el correo de notificación." };
                }
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task LogRejectedTransactionAsync(string sourceAccountNumber, decimal amount, string reason)
        {
            var sourceAccount = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (sourceAccount != null)
            {
                var rejectedTransaction = new Transaction
                {
                    AccountNumber = sourceAccount.AccountNumber,
                    Type = TransactionType.Debito,
                    Amount = amount,
                    Status = TransactionStatus.Rechazada,
                    Description = reason,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(rejectedTransaction);
                await _transactionRepository.SaveChangesAsync();
            }
        }
    }
}