using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Enums
{
    //Estado de pago de una cuota individual dentro de la tabla de amortización
    public enum InstallmentPaymentStatus
    {
        Pendiente = 1,
        ParcialmentePagada = 2,
        Pagada = 3
    }
}
