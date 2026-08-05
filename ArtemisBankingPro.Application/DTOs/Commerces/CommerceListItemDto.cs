
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Commerces
{
    public class CommerceListItemDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; }
        public string RNC { get; set; }
        public bool IsActive { get; set; }
    }
}
