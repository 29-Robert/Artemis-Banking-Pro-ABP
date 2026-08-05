using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text;
using System.Threading.Tasks;



namespace ArtemisBankingPro.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IMapper _mapper;
        
        public CreditCardService(ICreditCardRepository creditCardRepository, IMapper mapper)
        {
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
        }

        public CreditCardService(ICreditCardRepository creditCardRepository)
        {
            _creditCardRepository = creditCardRepository;
        }

        public async Task<CreditCardResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, string adminId)
        {
            string cvc = new Random().Next(100, 999).ToString();
            string cardNumber = GenerateCreditCardNumber();
            DateTime expirationDate = DateTime.UtcNow.AddYears(3);

            var creditCard = new CreditCard
            {
                ClientId = request.ClientId,
                CardNumber = cardNumber,
                CreditLimit = request.CreditLimit,
                CurrentDebt = 0.00m,
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                CvcHash = HashString(cvc),
                Status = "Activa",
                AdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            await _creditCardRepository.AddAsync(creditCard);


            return new CreditCardResponseDto { /* Mapear DTO de respuesta */ };
        }

        public async Task UpdateCreditLimitAsync(int cardId, decimal newLimit)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) throw new Exception("La tarjeta seleccionada no existe.");
            if (card.Status == "Cancelada") throw new Exception("No se puede modificar una tarjeta cancelada.");
            if (newLimit < card.CurrentDebt) throw new Exception("El límite no puede ser inferior al monto adeudado actualmente.");

            card.CreditLimit = newLimit;
            await _creditCardRepository.UpdateAsync(card);

            
        }

        public async Task CancelCreditCardAsync(int cardId)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) throw new Exception("La tarjeta seleccionada no existe.");
            if (card.CurrentDebt > 0) throw new Exception("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.");

            card.Status = "Cancelada";
            await _creditCardRepository.UpdateAsync(card);
        }

        private string GenerateCreditCardNumber()
        {
            var rnd = new Random();
            var builder = new StringBuilder(16);
            for (int i = 0; i < 16; i++) builder.Append(rnd.Next(0, 9));
            return builder.ToString();
        }

        private string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
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
    }
}