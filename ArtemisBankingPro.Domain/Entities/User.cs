using ArtemisBankingPro.Domain.Common;

namespace ArtemisBankingPro.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Cedula { get; set; }
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public Role Role { get; set; }
        public ICollection<ConfirmationToken> ConfirmationTokens { get; set; }
        public ICollection<SavingsAccount> SavingsAccounts { get; set; }
        public int? CommerceId { get; set; }
        public Commerce? Commerce { get; set; }
    }
}