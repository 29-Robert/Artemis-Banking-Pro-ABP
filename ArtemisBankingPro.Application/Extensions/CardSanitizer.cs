namespace ArtemisBankingPro.Application.Extensions
{
    public static class CardSanitizer
    {
        public static string MaskPan(this string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
                return "xxxx";

            return new string('*', cardNumber.Length - 4) + cardNumber[^4..];
        }
    }
}
