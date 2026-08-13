using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Commands
{
    public class CancelCreditCardCommand : IRequest<Unit>
    {
        public int CardId { get; set; }
    }
}
