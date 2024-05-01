using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
