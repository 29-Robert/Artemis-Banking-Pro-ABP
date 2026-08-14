using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;

namespace ArtemisBankingPro.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreditCardService(
            ICreditCardRepository creditCardRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _creditCardRepository = creditCardRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

       
        //ASIGNAR TARJETA
        
        public async Task<CreditCardCreatedResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, int adminId)
        {
            var clientId = int.Parse(request.ClientId); 

            var client = await _userRepository.GetByIdAsync(clientId);
            if (client == null) throw new KeyNotFoundException("El cliente seleccionado no existe.");
            if (!client.IsActive) throw new InvalidOperationException("Solo se puede asignar tarjeta a clientes activos.");
            if (request.CreditLimit <= 0) throw new ArgumentException("El límite de crédito debe ser mayor a cero.");

           

            var cvc = RandomNumberGenerator.GetInt32(100, 1000).ToString();
            var cardNumber = await GenerateUniqueCreditCardNumberAsync();
            var expirationDate = DateTime.UtcNow.AddYears(3);

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
                CreatedAt = DateTime.UtcNow
            };

            await _creditCardRepository.AddAsync(creditCard);
            await _creditCardRepository.SaveChangesAsync();

            await _emailService.SendNotificationEmailAsync(
                client.Email,
                "Tarjeta de crédito asignada",
                $"Se le asignó una tarjeta de crédito terminada en {cardNumber[^4..]} con límite RD${request.CreditLimit:N2}.");
            // El correo NO incluye el CVC.

            var response = _mapper.Map<CreditCardCreatedResponseDto>(creditCard);
            response.Cvc = cvc; // única vez que el CVC en texto plano sale de este método
            return response;
        }

        // ============================================================
        // MODIFICAR LÍMITE
        // ============================================================
        public async Task UpdateCreditLimitAsync(int cardId, decimal newLimit)
        {
            var card = await _creditCardRepository.GetByIdWithDetailsAsync(cardId);
            if (card == null) throw new KeyNotFoundException("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new InvalidOperationException("No se puede modificar una tarjeta cancelada.");
            if (newLimit <= 0) throw new ArgumentException("El límite debe ser mayor que cero.");
            if (newLimit < card.CurrentDebt) throw new InvalidOperationException("El límite no puede ser inferior al monto adeudado.");

            card.CreditLimit = newLimit;
            await _creditCardRepository.UpdateAsync(card);
            await _creditCardRepository.SaveChangesAsync();

            await _emailService.SendNotificationEmailAsync(
                card.Client.Email,
                "Límite de tarjeta modificado",
                $"El límite de tu tarjeta terminada en {card.CardNumber[^4..]} ahora es RD${newLimit:N2}.");
        }

        // ============================================================
        // CANCELAR TARJETA
        // ============================================================
        public async Task CancelCreditCardAsync(int cardId)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) throw new KeyNotFoundException("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new InvalidOperationException("La tarjeta ya está cancelada.");
            if (card.CurrentDebt > 0) throw new InvalidOperationException("No se puede cancelar una tarjeta con deuda pendiente.");

            card.Status = "Cancelada";
            await _creditCardRepository.UpdateAsync(card);
            await _creditCardRepository.SaveChangesAsync();
        }

        // ============================================================
        // LISTADO PAGINADO
        // ============================================================
        public async Task<PagedResult<CreditCardResponseDto>> GetCreditCardsAsync(
            string? cedula, string? status, int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _creditCardRepository.SearchAsync(cedula, status, pageNumber, pageSize);

            return new PagedResult<CreditCardResponseDto>
            {
                Items = _mapper.Map<List<CreditCardResponseDto>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ============================================================
        // DETALLE POR ID
        // ============================================================
        public async Task<CreditCardResponseDto> GetCreditCardByIdAsync(int id)
        {
            var card = await _creditCardRepository.GetByIdWithDetailsAsync(id);
            if (card == null) throw new KeyNotFoundException("La tarjeta seleccionada no existe.");

            return _mapper.Map<CreditCardResponseDto>(card);
        }

        // ============================================================
        // Helpers privados
        // ============================================================
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

        public async Task<CreditCardResponseDto> GetCreditCardByIdAsync(int id)
        {
            var card = await _creditCardRepository.GetByIdAsync(id);
            if (card == null) throw new Exception("La tarjeta seleccionada no existe.");
            return _mapper.Map<CreditCardResponseDto>(card);
        }

       
    }
}