using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBankingPro.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreditCardService> _logger;

        public CreditCardService(
            ICreditCardRepository creditCardRepository,
            IGenericRepository<User> userRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _creditCardRepository = creditCardRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        
        // LISTADO PAGINADO
        
        public async Task<PagedResult<CreditCardResponseDto>> GetCreditCardsAsync(
            string? cedula, string? status, int pageNumber, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var client = (await _userRepository.GetAllAsync())
                    .FirstOrDefault(u => u.Cedula == cedula && u.Role?.Name == "Cliente");

                if (client == null)
                    throw new KeyNotFoundException("No existe un cliente registrado con esta cédula.");

                var clientCards = await _creditCardRepository.GetCardsByClientAsync(client.Id);
                if (!clientCards.Any())
                    throw new InvalidOperationException("Este cliente no tiene tarjetas de crédito registradas.");
            }

            
            var (items, totalCount) = await _creditCardRepository.SearchAsync(cedula, status, pageNumber, pageSize);

            return new PagedResult<CreditCardResponseDto>
            {
                Items = _mapper.Map<List<CreditCardResponseDto>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        
        // DETALLE CON CONSUMOS
        public async Task<CreditCardResponseDto> GetCreditCardByIdAsync(int id)
        {
            var card = await _creditCardRepository.GetByIdWithDetailsAsync(id);
            if (card == null) throw new KeyNotFoundException("La tarjeta seleccionada no existe.");

            var dto = _mapper.Map<CreditCardResponseDto>(card);
            dto.Consumptions = dto.Consumptions?
                .OrderByDescending(c => c.Date)
                .ToList();

            return dto;
        }


        // ASIGNAR TARJETA

        public async Task<CreditCardCreatedResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, int adminId)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId))
                throw new ArgumentException("Debe seleccionar un cliente para continuar.");

            var clientId = int.Parse(request.ClientId);

            var client = await _userRepository.GetByIdAsync(clientId);
            if (client == null) throw new KeyNotFoundException("El cliente seleccionado no existe.");
            if (!client.IsActive) throw new InvalidOperationException("Solo se puede asignar tarjetas de crédito a clientes activos.");
            if (request.CreditLimit <= 0) throw new ArgumentException("El límite de crédito debe ser mayor que cero.");

            var cvc = RandomNumberGenerator.GetInt32(100, 1000).ToString();
            var cardNumber = await GenerateUniqueCreditCardNumberAsync();
            var createdAt = DateTime.UtcNow;
            var expirationDate = createdAt.AddYears(3);

            var creditCard = new CreditCard
            {
                ClientId = clientId,
                CardNumber = cardNumber,
                CreditLimit = request.CreditLimit,
                CurrentDebt = 0m,
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                CvcHash = HashString(cvc),
                Status = "Activa",
                AdminId = adminId,
                CreatedAt = createdAt
            };

            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _creditCardRepository.AddAsync(creditCard);
                await _unitOfWork.CommitAsync(); 
            }
            catch
            {
                await _unitOfWork.RollbackAsync(); 
                throw;
            }

            _logger.LogInformation("Tarjeta de crédito ASIGNADA: Cliente ID: {ClientId}, Tarjeta: {CardNo}, Límite: RD$ {Limit:N2}, Admin: {AdminId}", clientId, cardNumber.MaskCardNumber(), request.CreditLimit, adminId);

            var response = _mapper.Map<CreditCardCreatedResponseDto>(creditCard);
            response.Cvc = cvc;

            
            try
            {
                await _emailService.SendNotificationEmailAsync(
                    client.Email,
                    "Nueva tarjeta de crédito asignada",
                    $"Se ha asignado una nueva tarjeta de crédito a su cuenta.\n" +
                    $"Tarjeta terminada en: {cardNumber[^4..]}\n" +
                    $"Límite aprobado: RD${request.CreditLimit:N2}\n" +
                    $"Fecha de expiración: {expirationDate:MM/yy}\n" +
                    $"Fecha de asignación: {createdAt:dd/MM/yyyy}\n" +
                    "Por seguridad, no comparta la información de su tarjeta con terceros.");
            }
            catch
            {
                response.EmailNotificationFailed = true;
            }

            return response;
        }

        // MODIFICAR LÍMITE
        public async Task<CreditCardResponseDto> UpdateCreditLimitAsync(int cardId, decimal newLimit)
        {
            var card = await _creditCardRepository.GetByIdWithDetailsAsync(cardId);
            if (card == null) throw new KeyNotFoundException("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new InvalidOperationException("No se puede modificar una tarjeta cancelada.");
            if (newLimit <= 0) throw new ArgumentException("El límite de la tarjeta debe ser mayor que cero.");
            if (newLimit < card.CurrentDebt)
                throw new InvalidOperationException("El límite de la tarjeta no puede ser inferior al monto adeudado actualmente.");

            card.CreditLimit = newLimit;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _creditCardRepository.UpdateAsync(card);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            _logger.LogInformation("Límite de tarjeta de crédito ACTUALIZADO: Tarjeta ID: {CardId}, Tarjeta: {CardNo}, Nuevo Límite: RD$ {Limit:N2}", cardId, card.CardNumber.MaskCardNumber(), newLimit);

            var modifiedAt = DateTime.UtcNow;
            var response = _mapper.Map<CreditCardResponseDto>(card);

            try
            {
                await _emailService.SendNotificationEmailAsync(
                    card.Client.Email,
                    "Modificación de límite de tarjeta",
                    $"El límite de su tarjeta de crédito terminada en {card.CardNumber[^4..]} ha sido actualizado.\n" +
                    $"Nuevo límite aprobado: RD${newLimit:N2}\n" +
                    $"Fecha de modificación: {modifiedAt:dd/MM/yyyy}\n" +
                    "Si usted no reconoce esta modificación, comuníquese con la entidad bancaria.");
            }
            catch
            {
                response.EmailNotificationFailed = true;
            }

            return response;
        }

        // CANCELAR TARJETA
        public async Task CancelCreditCardAsync(int cardId)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) throw new KeyNotFoundException("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new InvalidOperationException("La tarjeta ya está cancelada.");
            if (card.CurrentDebt > 0)
                throw new InvalidOperationException("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.");

            card.Status = "Cancelada";

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _creditCardRepository.UpdateAsync(card);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // Helpers privados

        private async Task<string> GenerateUniqueCreditCardNumberAsync()
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var number = GenerateCreditCardNumber();
                var existing = await _creditCardRepository.GetByCardNumberAsync(number);
                if (existing == null) return number;
            }
            throw new InvalidOperationException("No fue posible generar un número de tarjeta único.");
        }

        private static string GenerateCreditCardNumber()
        {
            var builder = new StringBuilder(16);
            for (var i = 0; i < 16; i++)
                builder.Append(RandomNumberGenerator.GetInt32(0, 10));
            return builder.ToString();
        }

        private static string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}