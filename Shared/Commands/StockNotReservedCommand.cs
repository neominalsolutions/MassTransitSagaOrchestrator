using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
    // Sipariş edilen ürünlerin Reserve Edilmediği durumda gönderilir.
    // Stock API dan State Machine API gönderilir.
    public class StockNotreservedCommand : CorrelatedBy<Guid>
    {
        public StockNotreservedCommand(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public string Message { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
