using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
    // Payment API de ödeme işlemini başlatmak için StockReserved edildiği durumda State Machine Service üzerinden gönderilir.
    // Payment işlemini başlatmak anlamına gelir.
    // Burada Kredi Kart Bilgileride tutulamalıdır. Örnek olduğu için ihmal edilmiştir.
    public class StartPaymentCommand : CorrelatedBy<Guid>
    {
        public StartPaymentCommand(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
