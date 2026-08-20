using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using DomainTransaction = ArtemisBankingPro.Domain.Entities.Transaction;
using DomainTransactionStatus = ArtemisBankingPro.Domain.Enums.TransactionStatus;

namespace ArtemisBankingPro.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<SavingsAccount> accountRepository,
        IGenericRepository<ConfirmationToken> tokenRepository,
        IGenericRepository<DomainTransaction> transactionRepository,
        IGenericRepository<Commerce> commerceRepository,
        IEmailService emailService,
        IMapper mapper) : IRequestHandler<CreateUserCommand, int>
    {
        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();
            if (users.Any(u => u.Username == request.Username || u.Email == request.Email || u.Cedula == request.Cedula))
            {
                throw new Exception("Ya existe un usuario con ese nombre de usuario, correo o cédula.");
            }

            if (request.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio && request.CommerceId.HasValue)
            {
                var commerce = await commerceRepository.GetByIdAsync(request.CommerceId.Value);
                if (commerce == null)
                {
                    throw new KeyNotFoundException($"No se encontró el comercio con el ID {request.CommerceId.Value}.");
                }

                var commerceHasUser = users.Any(u => u.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio && u.CommerceId == request.CommerceId.Value);
                if (commerceHasUser)
                {
                    throw new Exception("El comercio seleccionado ya tiene un usuario asociado. Solo se permite un usuario por comercio.");
                }
            }

            var user = mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.IsActive = false;

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var newUser = await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();

            if (newUser.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Cliente || newUser.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio)
            {
                var account = new SavingsAccount
                {
                    UserId = newUser.Id,
                    AccountNumber = new Random().Next(100000000, 999999999).ToString(),
                    Balance = request.InitialAmount,
                    IsPrincipal = true
                };
                var newAccount = await accountRepository.AddAsync(account);
                await accountRepository.SaveChangesAsync();

                if (request.InitialAmount > 0)
                {
                    var initialTransaction = new DomainTransaction
                    {
                        AccountNumber = newAccount.AccountNumber,
                        Type = TransactionType.Credito,
                        Amount = request.InitialAmount,
                        Description = "Monto inicial por apertura de cuenta principal",
                        Status = DomainTransactionStatus.Aprobada,
                        CreatedAt = DateTime.UtcNow
                    };
                    await transactionRepository.AddAsync(initialTransaction);
                    await transactionRepository.SaveChangesAsync();
                }
            }

            var activationToken = Guid.NewGuid().ToString();
            var confirmationToken = new ConfirmationToken
            {
                UserId = newUser.Id,
                Token = activationToken,
                Type = TokenType.Activacion,
                ExpirationDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };

            await tokenRepository.AddAsync(confirmationToken);
            await tokenRepository.SaveChangesAsync();

            transaction.Complete();

            var url = string.IsNullOrEmpty(request.ActivationUrlFormat) ? null : request.ActivationUrlFormat.Replace("TOKENPLACEHOLDER", activationToken);
            try
            {
                await emailService.SendActivationEmailAsync(newUser.Email, activationToken, url);
            }
            catch
            {
            }

            return newUser.Id;
        }
    }
}
