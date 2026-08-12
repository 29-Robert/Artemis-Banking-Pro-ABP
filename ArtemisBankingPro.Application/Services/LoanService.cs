/*using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly IMapper _mapper;
        private readonly ISavingsAccountService _accountService;
        public LoanService(ILoanRepository loanRepository,
                           ILoanInstallmentRepository installmentRepository,
                           IMapper mapper,
                           ISavingsAccountService accountService)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _mapper = mapper;
            _accountService = accountService;
        }
        // GET ALL 
        public async Task<List<LoanResponseDto>> GetAllLoansAsync()
        {
            var loans = await _loanRepository.GetAllAsync();
            return _mapper.Map<List<LoanResponseDto>>(loans);
        }
        // GET BY ID    
        public async Task<LoanResponseDto> GetLoanByIdAsync(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan == null) throw new Exception($"No se encontró un préstamo con Id {id}.");
            return _mapper.Map<LoanResponseDto>(loan);
        }
        // ASSIGN LOAN
        public async Task<LoanResponseDto> AssignLoanAsync(CreateLoanRequestDto request, string adminId)
        {
              
            if (await _loanRepository.HasActiveLoanAsync(request.ClientId))
                throw new Exception("El cliente ya tiene un préstamo activo. No se puede asignar otro.");
          
            var loan = new Loan
            {
                ClientId = request.ClientId,
                LoanNumber = GenerateUniqueLoanNumber(),
                CapitalAmount = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                Status = "Activo",
                AdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };
            var created = await _loanRepository.AddAsync(loan);
        
            var installments = await GenerateAmortizationTableAsync(
                request.CapitalAmount, request.AnnualInterestRate, request.TermInMonths);
            foreach (var inst in installments) inst.LoanId = created.Id;
            await _installmentRepository.AddRangeAsync(installments);


            
            await _accountService.CreditToMainAccountAsync(request.ClientId, request.CapitalAmount);
            return _mapper.Map<LoanResponseDto>(created);
        }
        // UPDATE RATE
        public async Task UpdateLoanRateAsync(int loanId, decimal newRate)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) throw new Exception("El préstamo seleccionado no existe.");
            if (loan.Status == "Completado") throw new Exception("No se puede modificar un préstamo completado.");
            loan.AnnualInterestRate = newRate;
            await _loanRepository.UpdateAsync(loan);
        }
        // AMORTIZACIÓN FRANCESA 
        public async Task<List<LoanInstallment>> GenerateAmortizationTableAsync(
            decimal capital, decimal annualRate, int months)
        {
            var installments = new List<LoanInstallment>();
            decimal monthlyRate = (annualRate / 100m) / 12m;

           
            decimal monthlyInstallment;
            if (monthlyRate == 0)
            {
                monthlyInstallment = capital / months;
            }
            else
            {
                decimal power = (decimal)Math.Pow((double)(1 + monthlyRate), months);
                monthlyInstallment = capital * (monthlyRate * power) / (power - 1);
            }
            monthlyInstallment = Math.Round(monthlyInstallment, 2);
            decimal pendingCapital = capital;
            DateTime dueDate = DateTime.UtcNow.AddMonths(1);
            for (int i = 1; i <= months; i++)
            {
                decimal interestAmount = Math.Round(pendingCapital * monthlyRate, 2);
                decimal capitalAmount = monthlyInstallment - interestAmount;
             
                if (i == months)
                {
                    capitalAmount = pendingCapital;
                    monthlyInstallment = capitalAmount + interestAmount;
                }
                pendingCapital -= capitalAmount;
                installments.Add(new LoanInstallment
                {
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    InstallmentAmount = Math.Round(monthlyInstallment, 2),
                    InterestAmount = interestAmount,
                    CapitalAmount = Math.Round(capitalAmount, 2),
                    PendingInstallmentAmount = Math.Round(monthlyInstallment, 2),
                    PaymentStatus = "Pendiente",
                    IsLate = false
                });
                dueDate = dueDate.AddMonths(1);
            }
            return await Task.FromResult(installments);
        }
        // HELPERS
        private static string GenerateUniqueLoanNumber()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }
    }

}
*/