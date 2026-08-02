using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities
{
    public class ConfirmationToken : BaseEntity
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public TokenType Type { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; }

        public User User { get; set; }
    }
}