namespace ArtemisBankingPro.Application.DTOs.Commerces
{
    public class UpdateCommerceDto
    {
        public string BusinessName { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}