using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ArtemisBankingPro.Application.Services
{
    public class PaymentService(
        ICommerceRepository commerceRepository,
        ICreditCardRepository creditCardRepository,
        ICreditCardConsumptionRepository creditCardConsumptionRepository,
        IGenericRepository<User> userRepository,
        ISavingsAccountRepository savingsAccountRepository,
        ITransactionRepository transactionRepository,
        IEmailService emailService,
        IValidator<ProcessPaymentRequestDto> validator) : IPaymentService
    {
        public async Task<TransactionResponseDto> ProcessPaymentAsync(int commerceId, ProcessPaymentRequestDto request, int currentUserId)
        {
            // 1. Validar request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. Validar Comercio
            var commerce = await commerceRepository.GetByIdWithUserAsync(commerceId);
            if (commerce == null)
            {
                throw new KeyNotFoundException($"El comercio con ID {commerceId} no existe.");
            }
            if (!commerce.IsActive)
            {
                throw new InvalidOperationException($"El comercio '{commerce.BusinessName}' se encuentra inactivo.");
            }

            // 3. Validar Tarjeta
            var card = await creditCardRepository.GetByCardNumberAsync(request.CardNumber);
            if (card == null)
            {
                throw new KeyNotFoundException($"La tarjeta de crédito {request.CardNumber} no existe.");
            }

            bool isApproved = true;
            string? rejectionReason = null;

            if (card.Status != "Activa")
            {
                isApproved = false;
                rejectionReason = "La tarjeta de crédito se encuentra inactiva.";
            }
            else if (IsCardExpired(card))
            {
                isApproved = false;
                rejectionReason = "La tarjeta de crédito se encuentra vencida.";
            }
            else if (HashString(request.Cvc) != card.CvcHash)
            {
                isApproved = false;
                rejectionReason = "El código CVC es incorrecto.";
            }
            else if (request.Amount > (card.CreditLimit - card.CurrentDebt))
            {
                isApproved = false;
                rejectionReason = "Fondos insuficientes (crédito no disponible).";
            }

            // 4. Registrar transacción rechazada si aplica
            if (!isApproved)
            {
                var consumption = new CreditCardConsumption
                {
                    CreditCardId = card.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = request.Amount,
                    CommerceName = commerce.BusinessName,
                    CommerceId = commerce.Id,
                    Description = request.Description,
                    Status = "RECHAZADA",
                    CreatedAt = DateTime.UtcNow
                };

                await creditCardConsumptionRepository.AddAsync(consumption);
                await creditCardConsumptionRepository.SaveChangesAsync();

                return new TransactionResponseDto
                {
                    Id = consumption.Id,
                    Status = Domain.Enums.TransactionStatus.Rechazada,
                    Amount = request.Amount,
                    TransactionDate = consumption.TransactionDate,
                    ErrorMessage = rejectionReason
                };
            }

            // 5. Procesar cargo aprobado (Atomico - ACID)
            string authCode = Guid.NewGuid().ToString("N")[..8].ToUpper();
            CreditCardConsumption approvedConsumption;

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Aumentar deuda
                card.CurrentDebt += request.Amount;
                await creditCardRepository.UpdateAsync(card);
                await creditCardRepository.SaveChangesAsync();

                // Registrar consumo
                approvedConsumption = new CreditCardConsumption
                {
                    CreditCardId = card.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = request.Amount,
                    CommerceName = commerce.BusinessName,
                    CommerceId = commerce.Id,
                    Description = request.Description,
                    Status = "APROBADA",
                    CreatedAt = DateTime.UtcNow
                };

                await creditCardConsumptionRepository.AddAsync(approvedConsumption);
                await creditCardConsumptionRepository.SaveChangesAsync();

                // Acreditación al comercio
                var allUsers = await userRepository.GetAllAsync();
                var commerceUser = allUsers.FirstOrDefault(u => u.CommerceId == commerceId);
                if (commerceUser == null)
                {
                    throw new InvalidOperationException("El comercio no tiene un usuario administrador asociado.");
                }

                var allAccounts = await savingsAccountRepository.GetAllAsync();
                var account = allAccounts.FirstOrDefault(a => a.UserId == commerceUser.Id && a.IsPrincipal);
                if (account == null)
                {
                    throw new InvalidOperationException("El comercio no tiene una cuenta de ahorros principal activa.");
                }

                account.Balance += request.Amount;
                await savingsAccountRepository.UpdateAsync(account);
                await savingsAccountRepository.SaveChangesAsync();

                // Registrar transaccion bancaria
                var bankTransaction = new ArtemisBankingPro.Domain.Entities.Transaction
                {
                    AccountNumber = account.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = request.Amount,
                    Description = $"Acreditación Hermes Pay: {request.Description}",
                    Status = ArtemisBankingPro.Domain.Enums.TransactionStatus.Aprobada,
                    PerformedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await transactionRepository.AddAsync(bankTransaction);
                await transactionRepository.SaveChangesAsync();

                transaction.Complete();
            }

            // 6. Enviar notificaciones asíncronas por correo (tolerante a fallos)
            try
            {
                var cardholder = card.Client ?? await userRepository.GetByIdAsync(card.ClientId);
                if (cardholder != null && !string.IsNullOrEmpty(cardholder.Email))
                {
                    await emailService.SendNotificationEmailAsync(
                        cardholder.Email,
                        "Consumo aprobado - Artemis Credit Card",
                        $"Se ha registrado un consumo por valor de RD${request.Amount:N2} en {commerce.BusinessName}. Detalle: {request.Description}. Código de Autorización: {authCode}."
                    );
                }

                if (!string.IsNullOrEmpty(commerce.Email))
                {
                    await emailService.SendNotificationEmailAsync(
                        commerce.Email,
                        "Pago recibido - Hermes Pay",
                        $"Su comercio ha recibido un pago por valor de RD${request.Amount:N2}. Detalle: {request.Description}. Código de Autorización: {authCode}."
                    );
                }
            }
            catch
            {
                // Fallas en el envío de correo no interrumpen la respuesta HTTP exitosa
            }

            return new TransactionResponseDto
            {
                Id = approvedConsumption.Id,
                Status = Domain.Enums.TransactionStatus.Aprobada,
                AuthorizationCode = authCode,
                Amount = request.Amount,
                TransactionDate = approvedConsumption.TransactionDate
            };
        }

        public async Task<PagedTransactionsDto> GetTransactionsAsync(int commerceId, int page, int limit)
        {
            var allConsumptions = await creditCardConsumptionRepository.GetAllAsync();
            var filtered = allConsumptions
                .Where(c => c.CommerceId == commerceId)
                .OrderByDescending(c => c.TransactionDate)
                .ToList();

            var totalCount = filtered.Count;
            var paginated = filtered
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var dataDtos = new List<PaymentTransactionDto>();
            foreach (var c in paginated)
            {
                var cardNo = "****************";
                var card = c.CreditCard ?? await creditCardRepository.GetByIdAsync(c.CreditCardId);
                if (card != null && card.CardNumber.Length >= 4)
                {
                    cardNo = $"************{card.CardNumber[^4..]}";
                }
                dataDtos.Add(new PaymentTransactionDto
                {
                    Id = c.Id,
                    CardNumber = cardNo,
                    Amount = c.Amount,
                    Description = c.Description ?? string.Empty,
                    Status = c.Status,
                    TransactionDate = c.TransactionDate
                });
            }

            var totalPages = limit == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)limit);

            return new PagedTransactionsDto
            {
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Data = dataDtos
            };
        }

        private static bool IsCardExpired(CreditCard card)
        {
            if (!int.TryParse(card.ExpirationYear, out var year) || !int.TryParse(card.ExpirationMonth, out var month))
            {
                return true;
            }

            var expirationDate = new DateTime(year, month, 1).AddMonths(1).AddSeconds(-1);
            return DateTime.UtcNow > expirationDate;
        }

        private static string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
