using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArtemisBankingPro.Application.Interfaces.Services {

    public interface ILoanService
    {
        Task<List<LoanResponseDto>> GetAllLoansAsync();
        Task<LoanResponseDto> GetLoanByIdAsync(int id);
        Task<LoanResponseDto> AssignLoanAsync(CreateLoanRequestDto request, string adminId);
        Task UpdateLoanRateAsync(int loanId, decimal newRate);
        Task<List<LoanInstallment>> GenerateAmortizationTableAsync(decimal capital, decimal annualRate, int months);
    }
}
    



