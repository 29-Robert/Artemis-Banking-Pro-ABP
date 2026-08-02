using ArtemisBankingPro.Domain.Common;

namespace ArtemisBankingPro.Domain.Entities
{
    public class Commerce : BaseEntity
    {
        public int UserId { get; set; }
        public string BusinessName { get; set; }
        public string RNC { get; set; }
        public bool IsActive { get; set; }
        public string PrincipalAccountNumber { get; set; }

        public User User { get; set; }
    }
}