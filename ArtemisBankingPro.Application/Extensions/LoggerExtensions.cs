using System;

namespace ArtemisBankingPro.Application.Extensions
{
    public static class LoggerExtensions
    {
        public static string MaskCardNumber(this string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return string.Empty;
            if (cardNumber.Length < 4) return "****";
            return $"****-****-****-{cardNumber[^4..]}";
        }

        public static string MaskCvc(this string cvc)
        {
            return "***";
        }
    }
}
