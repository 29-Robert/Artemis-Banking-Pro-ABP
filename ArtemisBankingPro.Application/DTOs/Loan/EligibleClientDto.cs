using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace ArtemisBankingPro.Application.DTOs.Loan
    {
        public class EligibleClientDto
        {
            public string Id { get; set; }
            public string Cedula { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public decimal TotalDebt { get; set; }
        }
    }

