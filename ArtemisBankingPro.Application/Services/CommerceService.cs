using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using FluentValidation;
using System.Transactions;

namespace ArtemisBankingPro.Application.Services
{
    public class CommerceService(
        ICommerceRepository commerceRepository,
        IGenericRepository<User> userRepository,
        IGenericRepository<SavingsAccount> accountRepository,
        IValidator<CreateCommerceDto> createValidator,
        IValidator<UpdateCommerceDto> updateValidator) : ICommerceService
    {
        public async Task ChangeStatusAsync(int id, bool isActive)
        {
            var commerce = await commerceRepository.GetByIdWithUserAsync(id);
            if(commerce == null)
            {
                throw new KeyNotFoundException($"No se encontró el comercio con el ID {id}.");
            }

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                commerce.IsActive = isActive;
                await commerceRepository.UpdateAsync(commerce);
                await commerceRepository.SaveChangesAsync();

                if (!isActive)
                {
                    if (commerce.User != null)
                    {
                        commerce.User.IsActive = false;
                        await userRepository.UpdateAsync(commerce.User);
                        await userRepository.SaveChangesAsync();
                    }
                }

                transaction.Complete();
            }
        }

        public async Task<CommerceListItemDto> CreateCommerceAsync(CreateCommerceDto dto)
        {
            var validationResult = await createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingRnc = await commerceRepository.GetByRncAsync(dto.RNC);
            if (existingRnc != null)
            {
                throw new InvalidOperationException($"El RNC '{dto.RNC}' ya está registrado.");
            }

            var existingEmail = await commerceRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException($"El correo electrónico '{dto.Email}' ya está registrado.");
            }

            string principalAccountNumber = new Random().Next(100000000, 999999999).ToString();

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var commerce = new Commerce
                {
                    BusinessName = dto.BusinessName,
                    RNC = dto.RNC,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    IsActive = true,
                    PrincipalAccountNumber = principalAccountNumber
                };

                await commerceRepository.AddAsync(commerce);
                await commerceRepository.SaveChangesAsync();

                transaction.Complete();

                return new CommerceListItemDto
                {
                    Id = commerce.Id,
                    BusinessName = commerce.BusinessName,
                    RNC = commerce.RNC,
                    IsActive = commerce.IsActive
                };
            }
        }

        public async Task<CommerceDetailDto?> GetCommerceByIdAsync(int id)
        {
            var c = await commerceRepository.GetByIdWithUserAsync(id);
            if (c == null) return null;

            return new CommerceDetailDto
            {
                Id = c.Id,
                BusinessName = c.BusinessName,
                RNC = c.RNC,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive,
                PrincipalAccountNumber = c.PrincipalAccountNumber
            };
        }

        public async Task<PagedCommerceResponseDto> GetPagedCommercesAsync(int page, int limit)
        {
            int sanitizedPage = page < 1 ? 1 : page;
            int sanitizedLimit = limit < 1 ? 10 : limit;

            var (data, totalRecords) = await commerceRepository.GetPagedAsync(sanitizedPage, sanitizedLimit);

            var commerceDtos = data.Select(c => new CommerceDetailDto
            {
                Id = c.Id,
                BusinessName = c.BusinessName,
                RNC = c.RNC,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive,
                PrincipalAccountNumber = c.PrincipalAccountNumber
            }).ToList();

            var totalPages = sanitizedLimit == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)sanitizedLimit);

            return new PagedCommerceResponseDto
            {
                CurrentPage = sanitizedPage,
                TotalPages = totalPages,
                TotalCount = totalRecords,
                Data = commerceDtos
            };
        }

        public async Task UpdateCommerceAsync(int id, UpdateCommerceDto dto)
        {
            var validationResult = await updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var commerce = await commerceRepository.GetByIdWithUserAsync(id);
            if (commerce == null)
            {
                throw new KeyNotFoundException($"No se encontró el comercio con el ID {id}.");
            }
            var existingRnc = await commerceRepository.GetByRncAsync(dto.RNC);
            if (existingRnc != null && existingRnc.Id != id)
            {
                throw new InvalidOperationException($"El RNC '{dto.RNC}' ya está en uso por otro comercio.");
            }
            var existingEmail = await commerceRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != id)
            {
                throw new InvalidOperationException($"El correo '{dto.Email}' ya está en uso por otro comercio.");
            }
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                commerce.BusinessName = dto.BusinessName;
                commerce.RNC = dto.RNC;
                commerce.Email = dto.Email;
                commerce.Phone = dto.Phone;
                commerce.Address = dto.Address;
                await commerceRepository.UpdateAsync(commerce);
                await commerceRepository.SaveChangesAsync();
                if (commerce.User != null)
                {
                    commerce.User.LastName = dto.BusinessName;
                    commerce.User.Username = dto.RNC;
                    commerce.User.Email = dto.Email;
                    commerce.User.PhoneNumber = dto.Phone;
                    commerce.User.Cedula = "COM-" + dto.RNC;
                    await userRepository.UpdateAsync(commerce.User);
                    await userRepository.SaveChangesAsync();
                }
                transaction.Complete();
            }
        }
    }
}
