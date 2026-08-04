using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Loan
{
    public class LoanInstallmentDto
    {
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
