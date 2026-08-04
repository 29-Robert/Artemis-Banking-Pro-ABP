using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System.Security.Cryptography;
using System.Text;



namespace ArtemisBankingPro.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
       

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
    }
}