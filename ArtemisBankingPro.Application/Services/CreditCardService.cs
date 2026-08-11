using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBankingPro.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreditCardService(
            ICreditCardRepository creditCardRepository,
            IGenericRepository<User> userRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _creditCardRepository = creditCardRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<List<CreditCardResponseDto>> GetAllCreditCardsAsync()
        {
            var cards = await _creditCardRepository.GetAllAsync();
            return _mapper.Map<List<CreditCardResponseDto>>(cards);
        }

        public async Task<CreditCardResponseDto> GetCreditCardByIdAsync(int id)
        {
            var card = await _creditCardRepository.GetByIdAsync(id);
            if (card == null) throw new Exception($"No se encontró una tarjeta con Id {id}.");

            return _mapper.Map<CreditCardResponseDto>(card);
        }

        public async Task<CreditCardResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, string adminId)
        {
            var client = await _userRepository.GetByIdAsync(int.Parse(request.ClientId));

            if (client == null) throw new Exception("El cliente seleccionado no existe.");
            if (!client.IsActive) throw new Exception("Solo se puede asignar tarjeta a clientes activos.");
            if (request.CreditLimit <= 0) throw new Exception("El límite de crédito debe ser mayor a cero.");

            var cvc = RandomNumberGenerator.GetInt32(100, 1000).ToString();
            var cardNumber = await GenerateUniqueCreditCardNumberAsync();
            var expirationDate = DateTime.UtcNow.AddYears(3);

            var creditCard = new CreditCard
            {
                ClientId = request.ClientId,
                CardNumber = cardNumber,
                CreditLimit = request.CreditLimit,
                CurrentDebt = 0m,
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                CvcHash = HashString(cvc),
                Status = "Activa",
                AdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            await _creditCardRepository.AddAsync(creditCard);
            await _creditCardRepository.SaveChangesAsync();

            await _emailService.SendNotificationEmailAsync(
                client.Email,
                "Tarjeta de crédito asignada",
                $"Se le asignó una tarjeta de crédito terminada en {cardNumber[^4..]} con límite RD${request.CreditLimit:N2}.");

            return _mapper.Map<CreditCardResponseDto>(creditCard);
        }

        public async Task UpdateCreditLimitAsync(int cardId, decimal newLimit)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) throw new Exception("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new Exception("No se puede modificar una tarjeta cancelada.");
            if (newLimit < card.CurrentDebt) throw new Exception("El límite no puede ser inferior al monto adeudado.");

            card.CreditLimit = newLimit;

            await _creditCardRepository.UpdateAsync(card);
            await _creditCardRepository.SaveChangesAsync();
        }

        public async Task CancelCreditCardAsync(int cardId)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) throw new Exception("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new Exception("La tarjeta ya está cancelada.");
            if (card.CurrentDebt > 0) throw new Exception("No se puede cancelar una tarjeta con deuda pendiente.");

            card.Status = "Cancelada";

            await _creditCardRepository.UpdateAsync(card);
            await _creditCardRepository.SaveChangesAsync();
        }

        private async Task<string> GenerateUniqueCreditCardNumberAsync()
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var number = GenerateCreditCardNumber();
                var existing = await _creditCardRepository.GetByCardNumberAsync(number);

                if (existing == null)
                {
                    return number;
                }
            }

            throw new Exception("No fue posible generar un número de tarjeta único.");
        }

        private static string GenerateCreditCardNumber()
        {
            var builder = new StringBuilder(16);

            for (var i = 0; i < 16; i++)
            {
                builder.Append(RandomNumberGenerator.GetInt32(0, 10));
            }

            return builder.ToString();
        }

        private static string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }

        public Task<PagedResult<CreditCardResponseDto>> GetCreditCardsAsync(string? cedula, string? status, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<CreditCardResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, int adminId)
        {
            throw new NotImplementedException();
        }
    }
}