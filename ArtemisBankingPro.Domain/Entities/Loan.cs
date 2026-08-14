using ArtemisBankingPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArtemisBankingPro.Domain.Entities
    {
        public class Loan : BaseEntity
        {
            public int ClientId { get; set; }
            public User? Client { get; set; }

            public string LoanNumber { get; set; }
            public decimal CapitalAmount { get; set; }
            public int TermInMonths { get; set; }
            public decimal AnnualInterestRate { get; set; }

            public string Status { get; set; }

            public int AdminId { get; set; }
            public User? Admin { get; set; }

            public ICollection<LoanInstallment> Installments { get; set; } = new List<LoanInstallment>();
        }
    
}


