using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ILoanService
    {
        Task<PagedResult<LoanResponseDto>> GetLoansAsync(string? cedula, string? status, int pageNumber, int pageSize);
        Task<LoanResponseDto> GetLoanByIdAsync(int id);
        Task<EligibleClientsResponseDto> GetEligibleClientsAsync(string? cedula, int pageNumber, int pageSize);
        Task<LoanResponseDto> AssignLoanAsync(CreateLoanRequestDto request, int adminId);
        Task<LoanResponseDto> UpdateInterestRateAsync(int loanId, decimal newAnnualRate);
    }
}