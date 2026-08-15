using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Loan
{
    public class LoanResponseDto
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; }
        public string ClientId { get; set; }
        public string ClientFullName { get; set; }
        public decimal CapitalAmount { get; set; }
        public int TermInMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public int PaidInstallments { get; set; }
        public bool EmailNotificationFailed { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalAmountToPay { get; set; }
        public string Status { get; set; }
        public string ClientPaymentStatus { get; set; } 
        public DateTime CreatedAt { get; set; }

        // Lista de cuotas 
        public List<LoanInstallmentDto> Amortization { get; set; }
    }

}
