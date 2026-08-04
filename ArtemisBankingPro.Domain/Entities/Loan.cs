using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public string ClientId { get; set; } 
        public string LoanNumber { get; set; } 
        public decimal CapitalAmount { get; set; }
        public int TermInMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public string Status { get; set; } 
        public string AdminId { get; set; } 
        public DateTime CreatedAt { get; set; }

        // Navegación
        public ICollection<LoanInstallment> Installments { get; set; }
    }

}
