namespace ArtemisBankingPro.Application.DTOs
{
    public class ClientDebtDto
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
    }
}
