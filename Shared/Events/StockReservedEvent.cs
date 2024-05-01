using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class StockReservedEvent : CorrelatedBy<Guid>
    {
        public StockReservedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
    }
}
