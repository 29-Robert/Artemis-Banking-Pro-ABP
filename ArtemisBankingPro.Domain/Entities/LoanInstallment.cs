using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Entities
{
    public class LoanInstallment
    {
        public int Id { get; set; }
        public int LoanId { get; set; } 
        public Loan Loan { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal CapitalAmount { get; set; }
        public decimal PendingInstallmentAmount { get; set; }
        public string PaymentStatus { get; set; } 
        public bool IsLate { get; set; }
    }
}
