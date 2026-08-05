using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Commerces
{
    public class CreateCommerceDto
    {
        public string BusinessName { get; set; }
        public string RNC { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
