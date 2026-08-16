using ArtemisBankingPro.Domain.Common;

namespace ArtemisBankingPro.Domain.Entities
{
    public class Commerce : BaseEntity
    {
        public string BusinessName { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PrincipalAccountNumber { get; set; } = string.Empty;

        public User? User { get; set; }
        public ICollection<CreditCardConsumption> Consumptions { get; set; } = new List<CreditCardConsumption>();
    }
}