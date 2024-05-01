using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
    // Ödeme alınamadığı durumda Payment API dan State Machine Service'e gönderilir.
    public class PaymentFailedCommand : CorrelatedBy<Guid>
    {
        public PaymentFailedCommand(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
        public string Message { get; set; }
    }
}
