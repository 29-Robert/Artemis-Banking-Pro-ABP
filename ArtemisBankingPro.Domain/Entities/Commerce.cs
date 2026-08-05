using ArtemisBankingPro.Domain.Common;

namespace ArtemisBankingPro.Domain.Entities
{
    public class Commerce : BaseEntity
    {
        public string BusinessName { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PrincipalAccountNumber { get; set; } = string.Empty;

        public User? User { get; set; }
    }
}