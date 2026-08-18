using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.DTOs.Transactions;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public BeneficiaryService(
            IBeneficiaryRepository beneficiaryRepository,
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<BeneficiaryDto> AddBeneficiaryAsync(int clientId, CreateBeneficiaryDto dto)
        {
            var targetAccount = await _accountRepository.GetByAccountNumberAsync(dto.AccountNumber);
            if (targetAccount == null)
                throw new Exception("La cuenta del beneficiario no existe.");

            if (targetAccount.Status == AccountStatus.Cancelada)
                throw new Exception("La cuenta del beneficiario se encuentra cancelada.");

            if (targetAccount.UserId == clientId)
                throw new Exception("No puedes agregarte a ti mismo como beneficiario.");

            var existing = await _beneficiaryRepository.GetByClientAsync(clientId);
            foreach (var b in existing)
            {
                if (b.BeneficiaryAccountNumber == dto.AccountNumber)
                    throw new Exception("Ya tienes a este beneficiario registrado.");
            }

            var beneficiary = new Beneficiary
            {
                ClientId = clientId,
                BeneficiaryAccountNumber = dto.AccountNumber,
                Alias = dto.Alias,
                CreatedAt = DateTime.UtcNow
            };

            await _beneficiaryRepository.AddAsync(beneficiary);
            await _beneficiaryRepository.SaveChangesAsync();

            return new BeneficiaryDto
            {
                Id = beneficiary.Id,
                AccountNumber = beneficiary.BeneficiaryAccountNumber,
                Alias = beneficiary.Alias
            };
        }

        public async Task RemoveBeneficiaryAsync(int id)
        {
            var beneficiary = await _beneficiaryRepository.GetByIdAsync(id);
            if (beneficiary == null)
                throw new Exception("El beneficiario no existe.");

            await _beneficiaryRepository.DeleteAsync(beneficiary);
            await _beneficiaryRepository.SaveChangesAsync();
        }

        public async Task TransferToBeneficiaryAsync(ExpressTransactionDto dto)
        {
            if (dto.Amount <= 0)
                throw new Exception("El monto a transferir debe ser mayor que cero.");

            if (dto.SourceAccountNumber == dto.DestinationAccountNumber)
                throw new Exception("La cuenta origen y destino no pueden ser la misma.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var srcAcc = await _accountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
                if (srcAcc == null)
                    throw new Exception("La cuenta de origen no existe.");

                if (srcAcc.Status == AccountStatus.Cancelada)
                    throw new Exception("La cuenta de origen se encuentra cancelada.");

                if (srcAcc.IsBlocked)
                    throw new Exception("La cuenta de origen se encuentra bloqueada.");

                if (srcAcc.Balance - srcAcc.BlockedAmount < dto.Amount)
                    throw new Exception("Fondos insuficientes en la cuenta de origen.");

                var srcUser = await _userRepository.GetByIdAsync(srcAcc.UserId);
                if (srcUser == null || !srcUser.IsActive)
                    throw new Exception("El usuario de la cuenta de origen no se encuentra activo.");

                var tgtAcc = await _accountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);
                if (tgtAcc == null)
                    throw new Exception("La cuenta de destino no existe.");

                if (tgtAcc.Status == AccountStatus.Cancelada)
                    throw new Exception("La cuenta de destino se encuentra cancelada.");

                if (tgtAcc.IsBlocked)
                    throw new Exception("La cuenta de destino se encuentra bloqueada.");

                var tgtUser = await _userRepository.GetByIdAsync(tgtAcc.UserId);
                if (tgtUser == null || !tgtUser.IsActive)
                    throw new Exception("El usuario de la cuenta de destino no se encuentra activo.");

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
                    Description = $"Transferencia a beneficiario hacia cuenta {dto.DestinationAccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(debitTx);

                var creditTx = new Transaction
                {
                    AccountNumber = dto.DestinationAccountNumber,
                    Type = TransactionType.Credito,
                    Amount = dto.Amount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia de beneficiario recibida desde cuenta {dto.SourceAccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(creditTx);

                await _unitOfWork.CommitAsync();

                try
                {
                    string body = $"Hola,\nSe ha realizado una transferencia a uno de tus beneficiarios registrados.\n" +
                                  $"Cuenta origen: {dto.SourceAccountNumber}\n" +
                                  $"Cuenta destino: {dto.DestinationAccountNumber}\n" +
                                  $"Monto transferido: RD$ {dto.Amount:N2}\n" +
                                  $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                    await _emailService.SendNotificationEmailAsync(srcUser.Email ?? "soporte@artemisbanking.com", "Transferencia a Beneficiario Realizada", body);
                }
                catch { }
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
