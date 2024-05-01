using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
    // Ödeme alındığı durumda Payment API dan State Machine Service Gönderilir.
    public class PaymentCompletedCommand : CorrelatedBy<Guid>
    {
        public PaymentCompletedCommand(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
    }
}
