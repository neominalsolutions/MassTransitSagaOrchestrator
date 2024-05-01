using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    // Hangi Order için Ne kadarlık bir tutar ödeyeceğimiz bilgisini tutar.
    // Burada Kredi Kart Bilgileride tutulamalıdır. Örnek olduğu için ihmal edilmiştir.
    public class PaymentStartedEvent : CorrelatedBy<Guid>
    {
        public PaymentStartedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
