using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainLoan = ArtemisBankingPro.Domain.Entities.Loan;

namespace ArtemisBankingPro.Application.Features.Loan.Queries
{
    public class GetLoansQueryHandler : IRequestHandler<GetLoansQuery, PagedResult<LoanResponseDto>>
    {
        private readonly IGenericRepository<DomainLoan> _loanRepository;

        public GetLoansQueryHandler(IGenericRepository<DomainLoan> loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<PagedResult<LoanResponseDto>> Handle(GetLoansQuery request, CancellationToken cancellationToken)
        {
            var query = await _loanRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(request.Cedula))
            {
                query = query.Where(l => l.Client != null && l.Client.Cedula != null && l.Client.Cedula.Contains(request.Cedula)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(l => l.Status != null && l.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int totalRecords = query.Count;

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => new LoanResponseDto
                {
                    Id = l.Id,
                    LoanNumber = l.LoanNumber,
                    ClientId = l.ClientId.ToString(),
                    CapitalAmount = l.CapitalAmount,
                    TermInMonths = l.TermInMonths,
                    AnnualInterestRate = l.AnnualInterestRate,
                    Status = l.Status,
                    ClientFullName = l.Client != null ? $"{l.Client.FirstName} {l.Client.LastName}".Trim() : string.Empty,

                    
                    CreatedAt = l.CreatedAt, 

                    MonthlyInstallment = l.Installments.FirstOrDefault()?.InstallmentAmount ?? 0m,

                    
                    PaidInstallments = l.Installments.Count(i => i.PaymentStatus == "Pagada"),

                    
                    PendingAmount = l.Installments.Where(i => i.PaymentStatus != "Pagada").Sum(i => i.PendingInstallmentAmount),

                    TotalAmountToPay = l.Installments.Sum(i => i.InstallmentAmount),
                    ClientPaymentStatus = l.Installments.Any(i => i.IsLate) ? "Atrasado" : "Al Día",
                    EmailNotificationFailed = false,

                    Amortization = l.Installments.Select(i => new LoanInstallmentDto
                    {
                        InstallmentNumber = i.InstallmentNumber,
                        DueDate = i.DueDate,
                        InstallmentAmount = i.InstallmentAmount,
                        InterestAmount = i.InterestAmount,
                        CapitalAmount = i.CapitalAmount,
                        PendingInstallmentAmount = i.PendingInstallmentAmount,
                        PaymentStatus = i.PaymentStatus,
                        IsLate = i.IsLate
                    }).ToList()
                })
                .ToList();

            return new PagedResult<LoanResponseDto>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalRecords,
                Items = items
            };
        }
    }
}